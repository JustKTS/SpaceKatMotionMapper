using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CSharpFunctionalExtensions;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKat.Shared.Defines;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Views;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;
using PlatformAbstractions;
using Dispatcher = Avalonia.Threading.Dispatcher;
using Log = Serilog.Log;

namespace SpaceKatMotionMapper.ViewModels;

// TODO: 后续或需要拆分该ViewModel以匹配UI的修改
public partial class KatMotionConfigViewModel : ViewModelBase
{
    public OtherConfigsViewModel? Parent { get; init; }

    [ObservableProperty] private bool _isDefault;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ProcessFilename))]
    private string _processPath = string.Empty;

    public Guid Id = Guid.NewGuid();

    public ObservableCollection<KatMotionsWithModeViewModel> KatMotionsWithMode { get; set; }
    public ObservableCollection<int> KatMotionsModeNums { get; } = [];

    private readonly IKatMotionActivateService _katMotionActivateService;
    private readonly IKatMotionFileService _katMotionFileService;
    private readonly IPopUpNotificationService _popUpNotificationService;
    private readonly TimeAndDeadZoneVMService? _timeAndDeadZoneVmService;
    private readonly IStorageProviderService _storageProviderService;
    private readonly RunningProgramSelectorViewModel _runningProgramSelectorVM;
    private readonly IKatMotionSemanticProfile _katMotionSemanticProfile;

    [ObservableProperty] private bool _isConfigNameEditing;
    public string ProcessFilename => Path.GetFileName(ProcessPath);

    [ObservableProperty] private bool _isActivated;

    [ObservableProperty] private bool _isCustomDeadZone;
    [ObservableProperty] private KatDeadZoneConfig _deadZoneConfig = new();

    [ObservableProperty] private bool _isCustomMotionTimeConfigs;
    [ObservableProperty] private KatMotionTimeConfigs _motionTimeConfigs = new();

    public bool IsAvailable => CheckIsAvailable();

    // 主要构造函数 - 支持依赖注入
    public KatMotionConfigViewModel(
        IKatMotionActivateService katMotionActivateService,
        IKatMotionFileService katMotionFileService,
        IPopUpNotificationService popUpNotificationService,
        IStorageProviderService storageProviderService,
        RunningProgramSelectorViewModel runningProgramSelectorVM,
        TimeAndDeadZoneVMService? timeAndDeadZoneVmService = null,
        MainProjectKatMotionSemanticRuleAssembler? katMotionSemanticRuleAssembler = null,
        IKatMotionTimeConfigAdjustmentPolicy? motionTimeConfigAdjustmentPolicy = null,
        IKatMotionSemanticProfile? katMotionSemanticProfile = null)
    {
        // 直接使用注入的服务，不再使用服务定位器
        _katMotionActivateService = katMotionActivateService;
        _katMotionFileService = katMotionFileService;
        _popUpNotificationService = popUpNotificationService;
        _storageProviderService = storageProviderService;
        _runningProgramSelectorVM = runningProgramSelectorVM;
        _katMotionSemanticProfile = katMotionSemanticProfile
                                  ?? new MainProjectKatMotionSemanticProfile(
                                      katMotionSemanticRuleAssembler,
                                      motionTimeConfigAdjustmentPolicy);

        // TimeAndDeadZoneVMService可以为null（用于测试）
        _timeAndDeadZoneVmService = timeAndDeadZoneVmService;

        KatMotionsWithMode = [];
        KatMotionsWithMode.CollectionChanged += (_, e) =>
        {
            // 处理新增项：订阅PropertyChanged
            if (e.NewItems != null)
            {
                foreach (KatMotionsWithModeViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }

            // 处理移除项：取消订阅避免内存泄漏
            if (e.OldItems == null) return;

            foreach (KatMotionsWithModeViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        };
        AddKatMotionsWithMode();
        UpdateKatMotionsModeNums();
        OnPropertyChanged(nameof(IsAvailable));
    }

    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(KeyActionWithCommandViewModel.IsAvailable))
        {
            OnPropertyChanged(nameof(IsAvailable));
        }
    }

    private bool CheckIsAvailable()
    {
        return KatMotionsWithMode.All(e0 => e0.IsAvailable);
    }

    // TODO:该方法会导致已选择的Mode丢失，目前在删除时添加了确认，还需思考修改方法
    private void UpdateKatMotionsModeNums()
    {
        KatMotionsModeNums.Clear();
        KatMotionsWithMode.Iter(e => KatMotionsModeNums.Add(e.ModeNum));
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        if (Parent is null)
        {
            return;
        }

        var index = Parent.KatMotionConfigGroups.IndexOf(this);
        Parent.RemoveCommand.Execute(index);
    }


    [RelayCommand]
    private void AddKatMotionsWithMode()
    {
        KatMotionsWithMode.Add(new KatMotionsWithModeViewModel(this, KatMotionsWithMode.Count));
        KatMotionsModeNums.Add(KatMotionsWithMode.Count-1);
        OnPropertyChanged(nameof(IsAvailable)); 
    }

    [RelayCommand]
    private void RemoveKatMotionsWithMode(int index)
    {
        if (index <= 0 || index >= KatMotionsWithMode.Count) return;
        KatMotionsWithMode.RemoveAt(index);
        UpdateKatMotionsModeNums();
        OnPropertyChanged(nameof(IsAvailable));
    }

    [RelayCommand]
    private Task ActivateActions()
    {
        if (!IsActivated)
        {
            var ret = ToKatMotionConfigGroups();
            bool success;
            if (ret.IsSuccess)
            {
                _katMotionActivateService.ActivateKatMotions(ret.Value);
                success = true;
            }
            else
            {
                Log.Error(ret.Error, "[配置激活] 配置激活失败. ViewModel Id: {ViewModelId}, 错误: {ErrorMessage}", Id, ret.Error.Message);
                _popUpNotificationService.Pop(NotificationType.Error, ret.Error.Message);
                success = false;
            }

            IsActivated = true;
            if (!success)
            {
                IsActivated = false;
            }
        }
        else
        {
            var ret = ToKatMotionConfigGroups();
            bool success;
            if (ret.IsSuccess)
            {
                _katMotionActivateService.DeactivateKatMotions(ret.Value);
                _katMotionActivateService.ActivateKatMotions(ret.Value);
                success = true;
            }
            else
            {
                Log.Error(ret.Error, "[配置激活] 重新激活失败. ViewModel Id: {ViewModelId}, 错误: {ErrorMessage}", Id, ret.Error.Message);
                _popUpNotificationService.Pop(NotificationType.Error, ret.Error.Message);
                success = false;
            }

            // 只有在激活失败时才设置为 false，成功时保持激活状态
            if (!success)
            {
                IsActivated = false;
            }
        }

        return Task.CompletedTask;
    }

    private Result<bool, Exception> ValidateKatMotionConfig()
    {
        return _katMotionSemanticProfile.ValidatePreModeGraph(CreateKatMotionSemanticValidationContext());
    }

    private Result<bool, Exception> ValidateCrossModeConfigConsistency()
    {
        return _katMotionSemanticProfile.ValidatePostModeGraph(CreateKatMotionSemanticValidationContext());
    }

    private KatMotionConfigSemanticValidationContext CreateKatMotionSemanticValidationContext()
    {
        var items = KatMotionsWithMode
            .SelectMany(e => e.KatMotionGroups.SelectMany(g => g.Configs))
            .Select(katMotion => new KatMotionSemanticItem(
                katMotion.KatMotion,
                katMotion.ConfigMode,
                katMotion.KeyActionConfigGroup.ToKeyActionConfigList()))
            .ToList();

        return new KatMotionConfigSemanticValidationContext(items);
    }

    public Result<KatMotionConfigGroup, Exception> ToKatMotionConfigGroups()
    {
        var validationResult = ValidateKatMotionConfig();
        if (validationResult.IsFailure) return validationResult.Error;
        if (!validationResult.Value) return new Exception("转换失败");

        var modeGraphResult = ValidateKatMotionModeGraph();
        if (modeGraphResult.IsFailure) return modeGraphResult.Error;
        if (!modeGraphResult.Value) return new Exception("模式图验证失败");

        var crossModeResult = ValidateCrossModeConfigConsistency();
        if (crossModeResult.IsFailure) return crossModeResult.Error;
        if (!crossModeResult.Value) return new Exception("跨模式一致性验证失败");

        var katActions = KatMotionsWithMode.SelectMany(e => e.KatMotionGroups.SelectMany(g => g.Configs)).ToList();
        var configList = new List<KatMotionConfig>();
        foreach (var katMotion in katActions)
        {
            var config = katMotion.ToKatMotionConfig();
            configList.Add(config);
            var longDownConfig = katMotion.ToLongDownConfig();
            if (longDownConfig != null)
            {
                configList.Add(longDownConfig);
            }
        }

        var hasSingleActionMode = KatMotionsWithMode
            .SelectMany(e => e.KatMotionGroups.SelectMany(g => g.Configs))
            .Any(km => km.ConfigMode == KatConfigModeEnum.SingleAction && km.KatMotion != KatMotionEnum.Null);

        var finalIsCustomMotionTimeConfigs = IsCustomMotionTimeConfigs || hasSingleActionMode;

        var configGroups = new KatMotionConfigGroup(
            Id.ToString(), IsDefault, ProcessPath,
            configList,
            IsCustomDeadZone, DeadZoneConfig, finalIsCustomMotionTimeConfigs, MotionTimeConfigs); // Version 4

        return configGroups;

        Result<bool, Exception> ValidateKatMotionModeGraph()
        {
            var modeChangeValidator = new ModeChangeValidator();
            foreach (var configWithMode in KatMotionsWithMode)
            {
                modeChangeValidator.AddNode(configWithMode.ModeNum);
                foreach (var katActionConfig in configWithMode.KatMotionGroups.SelectMany(g => g.Configs))
                {
                    modeChangeValidator.AddEdge(configWithMode.ModeNum, katActionConfig.ToModeNum);
                }
            }

            var (cannotToModes, cannotReturnModes) = modeChangeValidator.Validate();
            var stringBuilder = new StringBuilder();
            if (cannotToModes.Count != 0)
                stringBuilder.Append($"模式 {string.Join("、", cannotToModes)} 无法从模式0到达!");
            if (cannotReturnModes.Count != 0)
                stringBuilder.Append($"\n模式 {string.Join("、", cannotReturnModes)} 无法返回到模式0");
            return cannotToModes.Count == 0 && cannotReturnModes.Count == 0
                ? true
                : new Exception(stringBuilder.ToString());
        }
    }

    /// <summary>
    /// 为单动作模式调整时间配置
    /// 单动作模式需要：长推判定时间50ms。
    /// 单个键盘或鼠标动作默认开启自动重复；多动作/组合默认仅触发一次。
    /// </summary>
    private KatMotionTimeConfigs AdjustMotionTimeConfigsForSingleActionMode()
    {
        var inputs = KatMotionsWithMode
            .SelectMany(e => e.KatMotionGroups.SelectMany(g => g.Configs))
            .Select(katMotion => new MotionTimeAdjustmentInput(
                katMotion.KatMotion,
                katMotion.ConfigMode,
                katMotion.ShouldEnableAutoRepeatForSingleActionByDefault()))
            .ToList();

        return _katMotionSemanticProfile.AdjustMotionTimeConfigs(MotionTimeConfigs, inputs);
    }

    public KatMotionTimeConfigs GetEffectiveMotionTimeConfigs()
    {
        return AdjustMotionTimeConfigsForSingleActionMode();
    }

    public bool HasSingleActionMode()
    {
        return KatMotionsWithMode
            .SelectMany(e => e.KatMotionGroups.SelectMany(g => g.Configs))
            .Any(km => km.ConfigMode == KatConfigModeEnum.SingleAction && km.KatMotion != KatMotionEnum.Null);
    }

    [RelayCommand]
    private Task EnableConfigGroup()
    {
        try
        {
            var configGroupRet = ToKatMotionConfigGroups();
            bool success;
            if (configGroupRet.IsSuccess)
            {
                _katMotionActivateService.ActivateKatMotions(configGroupRet.Value);
                success = true;
            }
            else
            {
                _popUpNotificationService.Pop(NotificationType.Error, configGroupRet.Error.Message);
                success = false;
            }

            return success
                ? Task.CompletedTask
                : Task.FromException(new Exception("启用失败"));
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }

    [RelayCommand]
    private void SaveToSystemConfig()
    {
        OnPropertyChanged(nameof(IsAvailable));
        if (!IsAvailable)
        {
            var errorMsg = GetIsAvailableErrorMessage();
            WeakReferenceMessenger.Default.Send(
                new PopupNotificationData(NotificationType.Error, errorMsg), Id);
            return;
        }

        var ret = SaveConfigGroupToSystemConfig();
        if (ret.IsSuccess)
        {
            var flag = ret.Value;
            if (flag)
            {
                WeakReferenceMessenger.Default.Send(
                    new PopupNotificationData(NotificationType.Success, "保存成功"), Id);

                if (IsActivated)
                {
                    ActivateActions();
                }

                return;
            }

            Log.Error("[配置保存] 配置保存失败. ViewModel Id: {ViewModelId}", Id);
            WeakReferenceMessenger.Default.Send(
                new PopupNotificationData(NotificationType.Error, "保存失败"), Id);
        }
        else
        {
            Log.Error(ret.Error, "[配置保存] 保存配置时发生异常. ViewModel Id: {ViewModelId}, 错误: {ErrorMessage}", Id, ret.Error.Message);
            WeakReferenceMessenger.Default.Send(
                new PopupNotificationData(NotificationType.Error, ret.Error.Message), Id);
        }
    }

    /// <summary>
    /// 获取 IsAvailable 检查失败时的详细错误信息
    /// </summary>
    private string GetIsAvailableErrorMessage()
    {
        var errorList = new List<string>();

        foreach (var modeWithMode in KatMotionsWithMode)
        {
            foreach (var motionGroup in modeWithMode.KatMotionGroups)
            {
                foreach (var config in motionGroup.Configs)
                {
                    // 直接检查属性，避免重复调用 IsAvailable（它内部会执行完整的 CheckIsAvailable）
                    var errors = new List<string>();

                    // 检查 KatMotion
                    if (config.KatMotion == KatMotionEnum.Null)
                    {
                        errors.Add("未选择运动方式");
                    }

                    // 检查 KatPressMode（非单动作模式）
                    if (config.ConfigMode != KatConfigModeEnum.SingleAction &&
                        config.KatPressMode == KatPressModeEnum.Null)
                    {
                        errors.Add("未选择按压模式");
                    }

                    // 检查 KeyActionConfigGroup
                    if (!config.KeyActionConfigGroup.IsAvailable)
                    {
                        errors.Add("按键配置不可用（未选择按键或按键配置不完整）");
                    }

                    // 检查重复配置（仅在进阶/专家模式下）
                    if (config.ConfigMode != KatConfigModeEnum.SingleAction)
                    {
                        var duplicateCount = motionGroup.Configs.Count(c =>
                            c.ConfigMode != KatConfigModeEnum.SingleAction &&
                            c.KatMotion == config.KatMotion &&
                            c.KatPressMode == config.KatPressMode &&
                            c.RepeatCount == config.RepeatCount);

                        if (duplicateCount > 1)
                        {
                            var pressModeName = config.KatPressMode != KatPressModeEnum.Null
                                ? config.KatPressMode.ToStringFast(useMetadataAttributes: true)
                                : "未设置";
                            errors.Add($"存在重复配置（{pressModeName} x{config.RepeatCount}）");
                        }
                    }

                    // 如果有错误，添加到错误列表
                    if (errors.Count <= 0) continue;
                    var motionName = config.KatMotion != KatMotionEnum.Null
                        ? config.KatMotion.ToStringFast(useMetadataAttributes: true)
                        : "未选择运动方式";

                    var configModeName = config.ConfigMode switch
                    {
                        KatConfigModeEnum.SingleAction => "单动作模式",
                        KatConfigModeEnum.Advanced => "进阶模式",
                        KatConfigModeEnum.Expert => "专家模式",
                        _ => "未知模式"
                    };

                    var location = $"模式{modeWithMode.ModeNum} - {motionName} ({configModeName})";
                    errorList.Add($"• [{location}]\n  {string.Join("\n  ", errors)}");
                }
            }
        }

        if (errorList.Count == 0)
        {
            return "存在配置错误，请检查";
        }

        return $"以下配置存在问题，请修复后再保存：\n\n{string.Join("\n\n", errorList)}";
    }

    private Result<bool, Exception> SaveConfigGroupToSystemConfig()
    {
        try
        {
            var configGroupRet = ToKatMotionConfigGroups();

            if (configGroupRet.IsSuccess)
            {
                var configGroup = configGroupRet.Value;
                var saveResult = IsDefault
                    ? _katMotionFileService.SaveDefaultConfigGroup(configGroup)
                    : _katMotionFileService.SaveConfigGroupToSysConf(configGroup);

                return saveResult;
            }
            else
            {
                return configGroupRet.Error;
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[配置保存] 保存配置组时发生未捕获异常. ViewModel Id: {ViewModelId}", Id);
            return e;
        }
    }

    [RelayCommand]
    private async Task SaveToFile()
    {
        var ret = await SaveConfigGroupToFileAsync();
        if (ret.IsFailure)
        {
            _popUpNotificationService.Pop(NotificationType.Error, ret.Error.Message);
        }
    }

    private async Task<Result<bool, Exception>> SaveConfigGroupToFileAsync()
    {
        try
        {
            var configGroupResult = ToKatMotionConfigGroups();
            if (configGroupResult.IsFailure) return configGroupResult.Error;
            return await SaveConfigGroupToFileAsync(configGroupResult.Value);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private async Task<Result<bool, Exception>> SaveConfigGroupToFileAsync(KatMotionConfigGroup configGroup)
    {
        var storageProvider = _storageProviderService.GetStorageProvider();

        var suggestedName = IsDefault ? "全局配置" : Path.GetFileNameWithoutExtension(configGroup.ProcessPath);
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = ".json", FileTypeChoices = [FilePickerFileTypeDefines.Json],
            ShowOverwritePrompt = true, SuggestedFileName = suggestedName, Title = "另存为配置文件"
        });

        return file == null
            ? false
            : _katMotionFileService.SaveConfigGroup(configGroup, file.Path.LocalPath);
    }

    [RelayCommand]
    private async Task<bool> LoadFromFile()
    {
        var storageProvider = _storageProviderService.GetStorageProvider();
        if (storageProvider == null) return false; // 在测试环境中可能为 null

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypeDefines.Json], Title = "配置文件" });
        if (files.Count == 0)
        {
            return false;
        }

        var file = files[0];
        var configGroup = _katMotionFileService.LoadConfigGroup(file.Path.LocalPath);
        bool ret;
        if (configGroup.IsSuccess)
        {
            ret = LoadFromConfigGroup(configGroup.Value);
        }
        else
        {
            _popUpNotificationService.Pop(NotificationType.Error, configGroup.Error.Message);
            ret = false;
        }

        return ret;
    }

    public bool LoadFromConfigGroup(KatMotionConfigGroup configGroup)
    {
        try
        {
            Id = Guid.Parse(configGroup.Guid);
            ProcessPath = configGroup.ProcessPath;
            IsCustomDeadZone = configGroup.IsCustomDeadZone;
            DeadZoneConfig = configGroup.DeadZoneConfig;
            IsCustomMotionTimeConfigs = configGroup.IsCustomMotionTimeConfigs;
            MotionTimeConfigs = configGroup.MotionTimeConfigs;

            KatMotionsWithMode.Clear();

            // 先检测并合并简单模式的配对配置
            var (processedMotions, simpleModeKeys) = MergeSimpleModeConfigs(configGroup.Motions);

            var configGroupWithMode = processedMotions.GroupBy(e => e.ModeNum);
            foreach (var groupWithMode in configGroupWithMode)
            {
                var katActionsWithModeVm = new KatMotionsWithModeViewModel(this, groupWithMode.Key);
                katActionsWithModeVm.KatMotionGroups.Clear();

                // 按 KatMotion 分组
                var groupedByMotion = groupWithMode.GroupBy(e => e.Motion.Motion);
                foreach (var motionGroup in groupedByMotion)
                {
                    var katMotionGroup = new KatMotionGroupViewModel(katActionsWithModeVm, motionGroup.Key);
                    katMotionGroup.Configs.Clear();

                    foreach (var katActionWithMode in motionGroup)
                    {
                        var katActionViewModel = new KatMotionViewModel(katMotionGroup, 0);
                        var ret = katActionViewModel.LoadFromKatMotionConfig(katActionWithMode);

                        // 检查是否是简单模式配置
                        var key = $"{katActionWithMode.Motion.Motion}_{katActionWithMode.Motion.RepeatCount}_{katActionWithMode.ModeNum}_{katActionWithMode.Motion.KatPressMode}";
                        if (simpleModeKeys.Contains(key))
                        {
                            katActionViewModel.IsAdvancedMode = false;
                        }

                        if (!ret) return false;
                        katMotionGroup.Configs.Add(katActionViewModel);
                    }

                    // 同步 FirstConfigMode 以反映实际加载的配置模式
                    katMotionGroup.FirstConfigMode = katMotionGroup.Configs.FirstOrDefault()?.ConfigMode ?? KatConfigModeEnum.SingleAction;

                    katActionsWithModeVm.KatMotionGroups.Add(katMotionGroup);
                }

                KatMotionsWithMode.Add(katActionsWithModeVm);
            }

            UpdateKatMotionsModeNums();

            // Compatibility: single-action config requires custom time settings to be enabled.
            if (!IsCustomMotionTimeConfigs && HasSingleActionMode())
            {
                IsCustomMotionTimeConfigs = true;
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "[{ViewModel}] Failed to load config from group", nameof(KatMotionConfigViewModel));
            return false;
        }

    }

    /// <summary>
    /// 检测并合并简单模式的长推保持和长推结束配置
    /// </summary>
    private (List<KatMotionConfig> motions, System.Collections.Generic.HashSet<string> simpleModeKeys) MergeSimpleModeConfigs(List<KatMotionConfig> motions)
    {
        var result = new List<KatMotionConfig>();
        var processedKeys = new System.Collections.Generic.HashSet<string>();
        var simpleModeKeys = new System.Collections.Generic.HashSet<string>();

        foreach (var motion in motions)
        {
            var key = $"{motion.Motion.Motion}_{motion.Motion.RepeatCount}_{motion.ModeNum}";

            // 如果已经处理过，跳过
            if (processedKeys.Contains(key))
            {
                continue;
            }

            // 查找对应的 LongDown 配置
            if (motion.Motion.KatPressMode == KatPressModeEnum.LongReach)
            {
                var matchingLongDown = motions.FirstOrDefault(m =>
                    m.Motion.Motion == motion.Motion.Motion &&
                    m.Motion.KatPressMode == KatPressModeEnum.LongDown &&
                    m.Motion.RepeatCount == motion.Motion.RepeatCount &&
                    m.ModeNum == motion.ModeNum);

                if (matchingLongDown != null)
                {
                    // 检测是否是自动生成的配对（LongDown 的描述以"松开:"开头且与 LongReach 的描述匹配）
                    if (matchingLongDown.KeyActionsDescription.StartsWith("松开:") &&
                        motion.KeyActionsDescription != "" &&
                        matchingLongDown.KeyActionsDescription == $"松开: {motion.KeyActionsDescription}")
                    {
                        // 这是简单模式的配对，只保留 LongReach 配置
                        result.Add(motion);
                        simpleModeKeys.Add($"{key}_LongReach");
                        processedKeys.Add(key);
                        processedKeys.Add($"{key}_LongDown");
                        continue;
                    }
                }
            }

            // 其他情况保持原样
            result.Add(motion);
            processedKeys.Add(key);
        }

        return (result, simpleModeKeys);
    }
    [RelayCommand]
    private async Task OpenRunningProgramSelector()
    {
        var ret = await Dialog.ShowCustomAsync<RunningProgramSelector, RunningProgramSelectorViewModel, object?>(
            _runningProgramSelectorVM, null, RunningProgramSelectorViewModel.DialogOptions);
        if (ret is not ForeProgramInfo info) return;
        await Dispatcher.UIThread.InvokeAsync(() => { ProcessPath = info.ProcessFileAddress; });
    }
    
    [RelayCommand]
    private async Task<bool> SelectProcessPath()
    {
        var storageProvider = _storageProviderService.GetStorageProvider();

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypeDefines.Exe], Title = "程序文件" });
        if (files.Count == 0)
        {
            return false;
        }

        var file = files[0];
        await Dispatcher.UIThread.InvokeAsync(() => { ProcessPath = file.Path.LocalPath; });
        return true;
    }

    [RelayCommand]
    private async Task SetTimeAndDeadZone()
    {
        if (_timeAndDeadZoneVmService == null)
        {
            _popUpNotificationService.Pop(NotificationType.Warning, "时间和死区配置服务不可用");
            return;
        }

        if (IsDefault)
        {
            _timeAndDeadZoneVmService.UpdateByDefault();
        }
        else
        {
            _timeAndDeadZoneVmService.UpdateByGuid(Id);
        }

        await _timeAndDeadZoneVmService.ShowDialogAsync();
    }


    # region 开启独立配置窗口

    private static readonly Dictionary<Guid, KatMotionGroupConfigWindow> OpenEditWindows = [];

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (OpenEditWindows.TryGetValue(Id, out var existingWindow))
        {
            existingWindow.WindowState = WindowState.Normal;
            existingWindow.Activate();
            return;
        }

        var window = App.GetRequiredService<KatMotionGroupConfigWindow>();
        window.DataContext = this;
        OpenEditWindows[Id] = window;
        window.Closed += (_, _) =>
        {
            if (OpenEditWindows.TryGetValue(Id, out var trackedWindow) && ReferenceEquals(trackedWindow, window))
            {
                OpenEditWindows.Remove(Id);
            }
        };
        window.Show();
    }

    #endregion
}
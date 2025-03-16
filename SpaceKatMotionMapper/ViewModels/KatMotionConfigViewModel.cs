using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Views;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace SpaceKatMotionMapper.ViewModels;

// TODO: 后续或需要拆分该ViewModel以匹配UI的修改
public partial class KatMotionConfigViewModel : ViewModelBase
{
    private readonly OtherConfigsViewModel? _parent;

    [ObservableProperty] private bool _isDefault;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ProcessFilename))]
    private string _processPath = string.Empty;

    public Guid Id = Guid.NewGuid();

    public ObservableCollection<KatMotionsWithModeViewModel> KatMotionsWithMode { get; set; }
    public ObservableCollection<int> KatMotionsModeNums { get; } = [];


    private readonly KatMotionActivateService _katMotionActivateService =
        App.GetRequiredService<KatMotionActivateService>();

    private readonly KatMotionFileService _katMotionFileService =
        App.GetRequiredService<KatMotionFileService>();

    private readonly PopUpNotificationService _popUpNotificationService =
        App.GetRequiredService<PopUpNotificationService>();

    private readonly TimeAndDeadZoneVMService _timeAndDeadZoneVmService =
        App.GetRequiredService<TimeAndDeadZoneVMService>();

    [ObservableProperty] private bool _isConfigNameEditing;
    public string ProcessFilename => Path.GetFileName(ProcessPath);

    [ObservableProperty] private bool _isActivated;

    [ObservableProperty] private bool _isCustomDeadZone;
    [ObservableProperty] private KatDeadZoneConfig _deadZoneConfig = new();

    [ObservableProperty] private bool _isCustomMotionTimeConfigs;
    [ObservableProperty] private KatMotionTimeConfigs _motionTimeConfigs = new();

    public bool IsAvailable => CheckIsAvailable();

#if DEBUG
    public KatMotionConfigViewModel() : this(null)
    {
    }
#endif

    public KatMotionConfigViewModel(OtherConfigsViewModel? parent = null)
    {
        _parent = parent;
        KatMotionsWithMode = [];
        KatMotionsWithMode.CollectionChanged += (sender, e) =>
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
        if (e.PropertyName == nameof(KeyActionViewModel.IsAvailable))
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
        if (_parent is null)
        {
            return;
        }

        var index = _parent.KatMotionConfigGroups.IndexOf(this);
        _parent.RemoveCommand.Execute(index);
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
            var ret2 = ret.Match(configGroup =>
            {
                _katMotionActivateService.ActivateKatMotions(configGroup);
                return true;
            }, ex =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            });

            IsActivated = true;
            if (!ret2)
            {
                IsActivated = false;
            }
        }
        else
        {
            var ret = ToKatMotionConfigGroups();
            var ret2 = ret.Match(configGroup =>
            {
                _katMotionActivateService.DeactivateKatMotions(configGroup);
                return true;
            }, ex =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                return false;
            });
            IsActivated = false;
            // 为了使失败时，开关状态保持打开
            // ReSharper disable once ConvertIfToOrExpression
            if (!ret2) IsActivated = true;
        }

        return Task.CompletedTask;
    }

    private Result<bool> ValidateKatMotionModeGraph()
    {
        var modeChangeValidator = new ModeChangeValidator();
        foreach (var configWithMode in KatMotionsWithMode)
        {
            modeChangeValidator.AddNode(configWithMode.ModeNum);
            foreach (var katActionConfig in configWithMode.KatMotions)
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
            : new Result<bool>(new Exception(stringBuilder.ToString()));
    }

    private Result<bool> ValidateKatMotionConfig()
    {
        List<string> pressKeys = [];
        List<string> releaseKeys = [];

        var katMotions = KatMotionsWithMode.SelectMany(e => e.KatMotions).ToList();

        foreach (var katMotion in katMotions)
        {
            pressKeys.AddRange(
                katMotion.KeyActionConfigGroup.ActionConfigGroups
                    .Where(e => e.PressMode == PressModeEnum.Press)
                    .Select(e => e.Key)
                    .ToArray());

            releaseKeys.AddRange(
                katMotion.KeyActionConfigGroup.ActionConfigGroups
                    .Where(e => e.PressMode == PressModeEnum.Release)
                    .Select(e => e.Key)
                    .ToArray());
        }

        var ret1 = pressKeys.All(key => releaseKeys.Contains(key));
        var ret2 = releaseKeys.All(key => pressKeys.Contains(key));
        if (ret1 && ret2) return true;
        var pressButNotReleaseKeys = pressKeys.Except(releaseKeys).ToArray();
        var releaseButNotPressKeys = releaseKeys.Except(pressKeys).ToArray();
        var exceptionStr = string.Empty;
        if (pressButNotReleaseKeys.Length != 0)
        {
            exceptionStr += $"按键{string.Join(",", pressButNotReleaseKeys)}配置了按下但没有被释放";
        }

        if (releaseButNotPressKeys.Length != 0)
        {
            exceptionStr += $"按键{string.Join(",", pressButNotReleaseKeys)}配置了释放但没有被按下";
        }

        return new Result<bool>(new Exception(exceptionStr));
    }

    public Result<KatMotionConfigGroup> ToKatMotionConfigGroups()
    {
        var ret = ValidateKatMotionConfig();
        return ret.Match(s =>
        {
            if (!s) return new Result<KatMotionConfigGroup>(new Exception("转换失败"));
            var retMode = ValidateKatMotionModeGraph();
            return retMode.Match(s1 =>
            {
                if (!s1) return new Result<KatMotionConfigGroup>(new Exception("转换失败"));
                var katActions = KatMotionsWithMode.SelectMany(e => e.KatMotions).ToList();

                var configGroups = new KatMotionConfigGroup(
                    Id.ToString(), IsDefault, ProcessPath,
                    katActions.Select(x => x.ToKatMotionConfig()).ToList(),
                    IsCustomDeadZone, DeadZoneConfig, IsCustomMotionTimeConfigs, MotionTimeConfigs);
                return configGroups;
            }, ex => new Result<KatMotionConfigGroup>(ex));
        }, ex => new Result<KatMotionConfigGroup>(ex));
    }

    [RelayCommand]
    private Task EnableConfigGroup()
    {
        try
        {
            var configGroupRet = ToKatMotionConfigGroups();
            return configGroupRet.Match(
                configGroup =>
                {
                    _katMotionActivateService.ActivateKatMotions(configGroup);
                    return true;
                },
                ex =>
                {
                    _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                    return false;
                })
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
            WeakReferenceMessenger.Default.Send(
                new PopupNotificationData(NotificationType.Error, "存在配置错误，请检查"), Id);
            return;
        }

        var ret = SaveConfigGroupToSystemConfig();
        _ = ret.Match(flag =>
        {
            if (flag)
            {
                WeakReferenceMessenger.Default.Send(
                    new PopupNotificationData(NotificationType.Success, "保存成功"), Id);
                if (!IsActivated) return true;
                ActivateActions(); // 如果启用了，则关闭后重新启用
                ActivateActions();

                return true;
            }

            WeakReferenceMessenger.Default.Send(
                new PopupNotificationData(NotificationType.Error, "保存失败"), Id);
            return false;
        }, ex =>
        {
            WeakReferenceMessenger.Default.Send(
                new PopupNotificationData(NotificationType.Error, ex.Message), Id);
            return false;
        });
    }

    private Result<bool> SaveConfigGroupToSystemConfig()
    {
        try
        {
            var configGroupRet = ToKatMotionConfigGroups();
            return configGroupRet.Match(
                configGroup =>
                    IsDefault
                        ? _katMotionFileService.SaveDefaultConfigGroup(configGroup)
                        : _katMotionFileService.SaveConfigGroupToSysConf(configGroup),
                ex => new Result<bool>(ex));
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }

    [RelayCommand]
    private async Task SaveToFile()
    {
        var ret = await SaveConfigGroupToFileAsync();
        ret.IfFail(ex => { _popUpNotificationService.Pop(NotificationType.Error, ex.Message); });
    }

    private async Task<Result<bool>> SaveConfigGroupToFileAsync()
    {
        try
        {
            var configGroupRet = ToKatMotionConfigGroups();
            return await configGroupRet.Match<Task<Result<bool>>>(
                async configGroup => await SaveConfigGroupToFileAsync(configGroup),
                ex => Task.FromResult(new Result<bool>(ex)));
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }

    private async Task<Result<bool>> SaveConfigGroupToFileAsync(KatMotionConfigGroup configGroup)
    {
        var storageProvider =
            App.GetRequiredService<IStorageProviderService>().GetStorageProvider();

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
        var storageProvider =
            App.GetRequiredService<IStorageProviderService>().GetStorageProvider();

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypeDefines.Json], Title = "配置文件" });
        if (files.Count == 0)
        {
            return false;
        }

        var file = files[0];
        var configGroup = _katMotionFileService.LoadConfigGroup(file.Path.LocalPath);
        var ret = configGroup.Match(LoadFromConfigGroup, exception =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, exception.Message);
                return false;
            }
        );
        return ret;
    }

    public bool LoadFromConfigGroup(KatMotionConfigGroup configGroup)
    {
        try
        {
            Id = Guid.Parse(configGroup.Guid);
            IsDefault = configGroup.IsDefault;
            ProcessPath = configGroup.ProcessPath;
            IsCustomDeadZone = configGroup.IsCustomDeadZone;
            DeadZoneConfig = configGroup.DeadZoneConfig;
            IsCustomMotionTimeConfigs = configGroup.IsCustomMotionTimeConfigs;
            MotionTimeConfigs = configGroup.MotionTimeConfigs;

            KatMotionsWithMode.Clear();
            var configGroupWithMode = configGroup.Motions.GroupBy(e => e.ModeNum);
            foreach (var groupWithMode in configGroupWithMode)
            {
                var katActionsWithModeVm = new KatMotionsWithModeViewModel(this, groupWithMode.Key);
                katActionsWithModeVm.KatMotions.Clear();
                foreach (var katActionWithMode in groupWithMode)
                {
                    var katActionViewModel = new KatMotionViewModel(katActionsWithModeVm, 0);
                    var ret = katActionViewModel.LoadFromKatMotionConfig(katActionWithMode);
                    if (!ret) return false;
                    katActionsWithModeVm.KatMotions.Add(katActionViewModel);
                }

                KatMotionsWithMode.Add(katActionsWithModeVm);
            }

            UpdateKatMotionsModeNums();
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    [RelayCommand]
    private async Task<bool> SelectProcessPath()
    {
        var storageProvider =
            App.GetRequiredService<IStorageProviderService>().GetStorageProvider();

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

    [RelayCommand]
    private void OpenEditDialog()
    {
        var window = App.GetRequiredService<KatMotionGroupConfigWindow>();
        window.DataContext = this;
        window.Show();
    }

    #endregion
}
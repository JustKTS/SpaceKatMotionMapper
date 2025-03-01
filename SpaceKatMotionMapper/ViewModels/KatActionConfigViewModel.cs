using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Services.Contract;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatActionConfigViewModel : ViewModelBase
{
    private readonly OtherConfigsViewModel? _parent;

    [ObservableProperty] private string _configName = "自定义配置";
    [ObservableProperty] private bool _isDefault;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ProcessFilename))]
    private string _processPath = string.Empty;

    public Guid Id = Guid.NewGuid();
    public ObservableCollection<KatActionsWithModeViewModel> KatActionsWithMode { get; set; }
    public ObservableCollection<int> KatActionsModeNums { get; } = [];


    private readonly KatActionActivateService _katActionActivateService =
        App.GetRequiredService<KatActionActivateService>();

    private readonly KatActionFileService _katActionFileService =
        App.GetRequiredService<KatActionFileService>();

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
    [ObservableProperty] private KatMotionTimeConfigs _motionTimeConfigs = new(false);

#if DEBUG
    public KatActionConfigViewModel() : this(null)
    {
    }
#endif

    public KatActionConfigViewModel(OtherConfigsViewModel? parent = null)
    {
        _parent = parent;
        KatActionsWithMode = [new KatActionsWithModeViewModel(this, 0)];
        UpdateKatActionsModeNums();
    }

    private void UpdateKatActionsModeNums()
    {
        KatActionsModeNums.Clear();
        KatActionsWithMode.Iter(e => KatActionsModeNums.Add(e.ModeNum));
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        if (_parent is null)
        {
            return;
        }

        var index = _parent.KatActionConfigGroups.IndexOf(this);
        _parent.RemoveCommand.Execute(index);
    }


    [RelayCommand]
    private void AddKatActionsWithMode()
    {
        KatActionsWithMode.Add(new KatActionsWithModeViewModel(this, KatActionsWithMode.Count));
        UpdateKatActionsModeNums();
    }

    [RelayCommand]
    private void RemoveKatActionsWithMode(int index)
    {
        if (index <= 0 || index >= KatActionsWithMode.Count) return;
        KatActionsWithMode.RemoveAt(index);
        UpdateKatActionsModeNums();
    }

    [RelayCommand]
    private Task ActivateActions()
    {
        if (!IsActivated)
        {
            var ret = ToKatActionConfigGroups();
            var ret2 = ret.Match(configGroup =>
            {
                _katActionActivateService.ActivateKatActions(configGroup);
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
            var ret = ToKatActionConfigGroups();
            var ret2 = ret.Match(configGroup =>
            {
                _katActionActivateService.DeactivateKatActions(configGroup);
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

    private Result<bool> ValidateKatActionConfig()
    {
        List<string> pressKeys = [];
        List<string> releaseKeys = [];
        List<int> toModes = [];
        List<int> modeNums = [];
        
        var katActions = KatActionsWithMode.SelectMany(e => e.KatActions).ToList();

        foreach (var katAction in katActions)
        {
            pressKeys.AddRange(
                katAction.KeyActionConfigGroup.ActionConfigGroups
                    .Where(e => e.PressMode == PressModeEnum.Press)
                    .Select(e => e.Key)
                    .ToArray());

            releaseKeys.AddRange(
                katAction.KeyActionConfigGroup.ActionConfigGroups
                    .Where(e => e.PressMode == PressModeEnum.Release)
                    .Select(e => e.Key)
                    .ToArray());
            modeNums.Add(katAction.ModeNum);
            toModes.Add(katAction.ToModeNum);
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

        // var existButNotToModeNums = modeNums.Except(toModes).ToArray();
        // if (existButNotToModeNums.Length != 0)
        // {
        //     exceptionStr += $"模式{string.Join(",", existButNotToModeNums)}没有按键动作跳转，请检查配置！";
        // }
        
        return new Result<bool>(new Exception(exceptionStr));
    }

    public Result<KatActionConfigGroup> ToKatActionConfigGroups()
    {
        var ret = ValidateKatActionConfig();
        return ret.Match(f =>
        {
            if (!f) return new Result<KatActionConfigGroup>(new Exception("转换失败"));
            var katActions = KatActionsWithMode.SelectMany(e => e.KatActions).ToList();

            var configGroups = new KatActionConfigGroup(
                Id.ToString(), IsDefault, ConfigName, ProcessPath,
                katActions.Select(x => x.ToKatActionConfig()).ToList(),
                IsCustomDeadZone, DeadZoneConfig, IsCustomMotionTimeConfigs, MotionTimeConfigs);
            return configGroups;
        }, ex => new Result<KatActionConfigGroup>(ex));
    }

    [RelayCommand]
    private Task EnableConfigGroup()
    {
        try
        {
            var configGroupRet = ToKatActionConfigGroups();
            return configGroupRet.Match(
                configGroup =>
                {
                    _katActionActivateService.ActivateKatActions(configGroup);
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
    private Task SaveToSystemConfig()
    {
        var ret = SaveConfigGroupToSystemConfig();
        return ret.IsSuccess ? Task.CompletedTask : Task.FromException(new Exception("保存到配置文件失败"));
    }

    private Result<bool> SaveConfigGroupToSystemConfig()
    {
        try
        {
            var configGroupRet = ToKatActionConfigGroups();
            return configGroupRet.Match(
                configGroup =>
                    IsDefault
                        ? _katActionFileService.SaveDefaultConfigGroup(configGroup)
                        : _katActionFileService.SaveConfigGroupToSysConf(configGroup),
                ex =>
                {
                    _popUpNotificationService.Pop(NotificationType.Error, ex.Message);
                    return false;
                });
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
            var configGroupRet = ToKatActionConfigGroups();
            return await configGroupRet.Match<Task<Result<bool>>>(
                async configGroup => await SaveConfigGroupToFileAsync(configGroup),
                ex => Task.FromResult(new Result<bool>(ex)));
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }

    private async Task<Result<bool>> SaveConfigGroupToFileAsync(KatActionConfigGroup configGroup)
    {
        var storageProvider =
            App.GetRequiredService<IStorageProviderService>().GetStorageProvider();

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = ".json", FileTypeChoices = [FilePickerFileTypeDefines.Json],
            ShowOverwritePrompt = true, SuggestedFileName = configGroup.Name, Title = "另存为配置文件"
        });

        return file == null
            ? false
            : _katActionFileService.SaveConfigGroup(configGroup, file.Path.LocalPath);
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
        var configGroup = _katActionFileService.LoadConfigGroup(file.Path.LocalPath);
        var ret = configGroup.Match(LoadFromConfigGroup, exception =>
            {
                _popUpNotificationService.Pop(NotificationType.Error, exception.Message);
                return false;
            }
        );
        return ret;
    }

    public bool LoadFromConfigGroup(KatActionConfigGroup configGroup)
    {
        try
        {
            Id = Guid.Parse(configGroup.Guid);
            ConfigName = configGroup.Name;
            IsDefault = configGroup.IsDefault;
            ProcessPath = configGroup.ProcessPath;
            IsCustomDeadZone = configGroup.IsCustomDeadZone;
            DeadZoneConfig = configGroup.DeadZoneConfig;
            IsCustomMotionTimeConfigs = configGroup.IsCustomMotionTimeConfigs;
            MotionTimeConfigs = configGroup.MotionTimeConfigs;

            KatActionsWithMode.Clear();
            var configGroupWithMode = configGroup.Actions.GroupBy(e => e.ModeNum);
            foreach (var groupWithMode in configGroupWithMode)
            {
                var katActionsWithModeVm = new KatActionsWithModeViewModel(this, groupWithMode.Key);
                katActionsWithModeVm.KatActions.Clear();
                foreach (var katActionWithMode in groupWithMode)
                {
                    var katActionViewModel = new KatActionViewModel(katActionsWithModeVm, 0);
                    var ret = katActionViewModel.LoadFromKatActionConfig(katActionWithMode);
                    if (!ret) return false;
                    katActionsWithModeVm.KatActions.Add(katActionViewModel);
                }

                KatActionsWithMode.Add(katActionsWithModeVm);
            }

            UpdateKatActionsModeNums();
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
}
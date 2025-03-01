using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class TimeAndDeadZoneSettingViewModel(
    KatActionRecognizeService katActionRecognizeService,
    KatActionConfigVMManageService katActionConfigVmManageService) : ViewModelBase
{
    [ObservableProperty] private bool _isDefault;
    [ObservableProperty] private bool _isTimeConfigEnable;
    [ObservableProperty] private bool _isDeadZoneConfigEnable;

    # region 当前Kat状态

    [ObservableProperty] private string _katMotion = string.Empty;
    [ObservableProperty] private string _pressMode = string.Empty;
    [ObservableProperty] private int _repeatCount;


    public void StartKatListening()
    {
        katActionRecognizeService.DataReceived += ListenKatStatus;
    }

    private void ListenKatStatus(object? _, KatAction data)
    {
        KatMotion = data.Motion.ToStringFast();
        PressMode = data.KatPressMode.ToStringFast();
        RepeatCount = data.RepeatCount;
    }

    public void StopKatListening()
    {
        katActionRecognizeService.DataReceived -= ListenKatStatus;
    }

    #endregion

    # region 状态

    public void UpdateByDefault()
    {
        IsDefault = true;
        IsDeadZoneConfigEnable = true;
        IsTimeConfigEnable = true;
    }
    
    public void UpdateById(Guid id)
    {
        IsDefault = false;
        var configRet = katActionConfigVmManageService.GetConfig(id);
        _ = configRet.Match(configVm =>
            {
                IsDeadZoneConfigEnable = configVm.IsCustomDeadZone;
                IsTimeConfigEnable = configVm.IsCustomMotionTimeConfigs;
                return true;
            }
            , _ => false);
    }

    # endregion
}
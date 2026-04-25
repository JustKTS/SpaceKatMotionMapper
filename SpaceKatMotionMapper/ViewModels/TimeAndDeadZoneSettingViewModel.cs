using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class TimeAndDeadZoneSettingViewModel(
    KatMotionRecognizeService katMotionRecognizeService,
    KatMotionConfigVMManageService katMotionConfigVmManageService,
    PopUpNotificationService popUpNotificationService) : ViewModelBase
{
    [ObservableProperty] private bool _isDefault;
    [ObservableProperty] private Guid _id = Guid.Empty;

    # region 当前Kat状态

    [ObservableProperty] private string _katMotion = string.Empty;
    [ObservableProperty] private string _pressMode = string.Empty;
    [ObservableProperty] private int _repeatCount;


    public void StartKatListening()
    {
        katMotionRecognizeService.DataReceived += ListenKatStatus;
    }

    private void ListenKatStatus(object? _, KatMotionWithTimeStamp data)
    {
        KatMotion = data.Motion.ToStringFast(useMetadataAttributes:true);
        PressMode = data.KatPressMode.ToStringFast(useMetadataAttributes:true);
        RepeatCount = data.RepeatCount;
    }

    public void StopKatListening()
    {
        katMotionRecognizeService.DataReceived -= ListenKatStatus;
    }

    #endregion

    # region 更新UI状态

    public void UpdateByDefault()
    {
        IsDefault = true;
        Id = Guid.Empty;
        IsDeadZoneConfigEnable = true;
        IsTimeConfigEnable = true;
    }

    public void UpdateById(Guid id)
    {
        IsDefault = false;
        Id = id;
        var configRet = katMotionConfigVmManageService.GetConfig(id);
        _ = configRet.Match(configVm =>
            {
                IsDeadZoneConfigEnable = configVm.IsCustomDeadZone;
                var hasSingleActionMode = configVm.HasSingleActionMode();
                IsTimeConfigEnable = configVm.IsCustomMotionTimeConfigs || hasSingleActionMode;
                if (hasSingleActionMode && !configVm.IsCustomMotionTimeConfigs)
                {
                    configVm.IsCustomMotionTimeConfigs = true;
                }
                return true;
            }
            , _ => false);
    }

    # endregion

    # region 启用/停止分应用设置

    [ObservableProperty] private bool _isDeadZoneConfigEnable;
    [ObservableProperty] private bool _isTimeConfigEnable;

    partial void OnIsDeadZoneConfigEnableChanged(bool value)
    {
        if (IsDefault || Id == Guid.Empty) return;
        var configRet = katMotionConfigVmManageService.GetConfig(Id);
        _ = configRet.Match(configVm =>
        {
            configVm.IsCustomDeadZone = value;
            return true;
        }, _ =>
        {
            popUpNotificationService.Pop(NotificationType.Warning, "分应用配置读取失败");
            return false;
        });
    }

    partial void OnIsTimeConfigEnableChanged(bool value)
    {
        if (IsDefault || Id == Guid.Empty) return;
        var configRet = katMotionConfigVmManageService.GetConfig(Id);
        _ = configRet.Match(configVm =>
        {
            if (!value && configVm.HasSingleActionMode())
            {
                IsTimeConfigEnable = true;
                popUpNotificationService.Pop(NotificationType.Warning, "存在单动作配置时，时间配置必须启用");
                return false;
            }
            configVm.IsCustomMotionTimeConfigs = value;
            return true;
        }, _ =>
        {
            popUpNotificationService.Pop(NotificationType.Warning, "分应用配置读取失败");
            return false;
        });
    }

    #endregion
}
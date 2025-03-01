using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class CommonConfigViewModel : ViewModelBase
{
    [ObservableProperty] private KatActionConfigViewModel _defaultKatActionConfig =
        App.GetRequiredService<KatActionConfigViewModel>();

    private readonly KatActionFileService _katActionFileService =
        App.GetRequiredService<KatActionFileService>();

    private readonly KatActionConfigVMManageService _katActionConfigVmManageService =
        App.GetRequiredService<KatActionConfigVMManageService>();
    private readonly KatMotionTimeConfigService _katMotionTimeConfigService =
        App.GetRequiredService<KatMotionTimeConfigService>();
    private readonly KatDeadZoneConfigService _katDeadZoneConfigService =
        App.GetRequiredService<KatDeadZoneConfigService>();

    private readonly ActivationStatusService _activationStatusService = App.GetRequiredService<ActivationStatusService>();

    public CommonConfigViewModel()
    {
        var configGroupRet = _katActionFileService.LoadDefaultConfigGroup();
        _ = configGroupRet.Match(cg =>
            {
                DefaultKatActionConfig.Id = Guid.Parse(cg.Guid);
                var ret2 = DefaultKatActionConfig.LoadFromConfigGroup(cg);
                if (!ret2) return ret2;
                DefaultKatActionConfig.ConfigName = "全局配置";
                DefaultKatActionConfig.IsDefault = true;
                if (!_activationStatusService.IsConfigGroupActivated(DefaultKatActionConfig.Id)) return true;
                DefaultKatActionConfig.ActivateActionsCommand.Execute(null);
                _katActionConfigVmManageService.RegisterDefaultConfig(DefaultKatActionConfig);
                _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
                return true;
            },
            ex =>
            {
                DefaultKatActionConfig.ConfigName = "全局配置";
                DefaultKatActionConfig.IsDefault = true;
                DefaultKatActionConfig.IsCustomDeadZone = true;
                DefaultKatActionConfig.DeadZoneConfig = new KatDeadZoneConfig();
                DefaultKatActionConfig.IsCustomMotionTimeConfigs = true;
                DefaultKatActionConfig.MotionTimeConfigs = new KatMotionTimeConfigs(true);
                _katActionConfigVmManageService.RegisterDefaultConfig(DefaultKatActionConfig);
                var cgRet = DefaultKatActionConfig.ToKatActionConfigGroups();
                return cgRet.Match(cg => _katActionFileService.SaveDefaultConfigGroup(cg),
                    e2 => new Result<bool>(e2));
            });
    }
}
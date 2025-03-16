using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class CommonConfigViewModel : ViewModelBase
{
    [ObservableProperty] private KatMotionConfigViewModel _defaultKatMotionConfig =
        App.GetRequiredService<KatMotionConfigViewModel>();

    private readonly KatMotionFileService _katMotionFileService =
        App.GetRequiredService<KatMotionFileService>();

    private readonly KatMotionConfigVMManageService _katMotionConfigVmManageService =
        App.GetRequiredService<KatMotionConfigVMManageService>();
    private readonly KatMotionTimeConfigService _katMotionTimeConfigService =
        App.GetRequiredService<KatMotionTimeConfigService>();
    private readonly KatDeadZoneConfigService _katDeadZoneConfigService =
        App.GetRequiredService<KatDeadZoneConfigService>();

    private readonly ActivationStatusService _activationStatusService = App.GetRequiredService<ActivationStatusService>();

    public CommonConfigViewModel()
    {
        var configGroupRet = _katMotionFileService.LoadDefaultConfigGroup();
        _ = configGroupRet.Match(cg =>
            {
                DefaultKatMotionConfig.Id = Guid.Parse(cg.Guid);
                var ret2 = DefaultKatMotionConfig.LoadFromConfigGroup(cg);
                if (!ret2) return ret2;
                DefaultKatMotionConfig.IsDefault = true;
                if (!_activationStatusService.IsConfigGroupActivated(DefaultKatMotionConfig.Id)) return true;
                DefaultKatMotionConfig.ActivateActionsCommand.Execute(null);
                _katMotionConfigVmManageService.RegisterDefaultConfig(DefaultKatMotionConfig);
                _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
                return true;
            },
            ex =>
            {
                DefaultKatMotionConfig.IsDefault = true;
                DefaultKatMotionConfig.IsCustomDeadZone = true;
                DefaultKatMotionConfig.DeadZoneConfig = new KatDeadZoneConfig();
                DefaultKatMotionConfig.IsCustomMotionTimeConfigs = true;
                DefaultKatMotionConfig.MotionTimeConfigs = new KatMotionTimeConfigs();
                _katMotionConfigVmManageService.RegisterDefaultConfig(DefaultKatMotionConfig);
                var cgRet = DefaultKatMotionConfig.ToKatMotionConfigGroups();
                return cgRet.Match(cg => _katMotionFileService.SaveDefaultConfigGroup(cg),
                    e2 => new Result<bool>(e2));
            });
    }
}
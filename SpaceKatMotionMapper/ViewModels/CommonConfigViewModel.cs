using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LanguageExt;
using SpaceKat.Shared.Services;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class CommonConfigViewModel : ViewModelBase
{
    [ObservableProperty] private KatMotionConfigViewModel _defaultKatMotionConfig;

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
        DefaultKatMotionConfig = App.GetRequiredService<KatMotionConfigViewModel>();
        var configGroupRet = _katMotionFileService.LoadDefaultConfigGroup();
        _ = configGroupRet.Bind<Unit>(cg =>
        {
            DefaultKatMotionConfig.Id = Guid.Parse(cg.Guid);
            var ret2 = DefaultKatMotionConfig.LoadFromConfigGroup(cg);
            if (!ret2) return Unit.Default;
            DefaultKatMotionConfig.IsDefault = true;
            _katMotionConfigVmManageService.RegisterDefaultConfig(DefaultKatMotionConfig);
            if (!_activationStatusService.IsConfigGroupActivated(DefaultKatMotionConfig.Id)) return Unit.Default;
            DefaultKatMotionConfig.ActivateActionsCommand.Execute(null);
            _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
            _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
            return Unit.Default;
        }).IfLeft(
            ex =>
            {
                DefaultKatMotionConfig.IsDefault = true;
                DefaultKatMotionConfig.IsCustomDeadZone = true;
                DefaultKatMotionConfig.DeadZoneConfig = new KatDeadZoneConfig();
                DefaultKatMotionConfig.IsCustomMotionTimeConfigs = true;
                DefaultKatMotionConfig.MotionTimeConfigs = new KatMotionTimeConfigs();
                _katMotionConfigVmManageService.RegisterDefaultConfig(DefaultKatMotionConfig);
                _ = DefaultKatMotionConfig.ToKatMotionConfigGroups().Bind(cg => _katMotionFileService.SaveDefaultConfigGroup(cg));
                return Unit.Default;
            }
        );
    }
}
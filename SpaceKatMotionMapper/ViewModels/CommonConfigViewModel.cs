using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CSharpFunctionalExtensions;
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
        if (configGroupRet.IsSuccess)
        {
            var cg = configGroupRet.Value;
            DefaultKatMotionConfig.Id = Guid.Parse(cg.Guid);
            var ret2 = DefaultKatMotionConfig.LoadFromConfigGroup(cg);
            if (ret2)
            {
                DefaultKatMotionConfig.IsDefault = true;
                _katMotionConfigVmManageService.RegisterDefaultConfig(DefaultKatMotionConfig);
                if (_activationStatusService.IsConfigGroupActivated(DefaultKatMotionConfig.Id))
                {
                    DefaultKatMotionConfig.ActivateActionsCommand.Execute(null);
                    _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                    _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
                }
            }
        }
        else
        {
            DefaultKatMotionConfig.IsDefault = true;
            DefaultKatMotionConfig.IsCustomDeadZone = true;
            DefaultKatMotionConfig.DeadZoneConfig = new KatDeadZoneConfig();
            DefaultKatMotionConfig.IsCustomMotionTimeConfigs = true;
            DefaultKatMotionConfig.MotionTimeConfigs = new KatMotionTimeConfigs();
            _katMotionConfigVmManageService.RegisterDefaultConfig(DefaultKatMotionConfig);
            var cgResult = DefaultKatMotionConfig.ToKatMotionConfigGroups();
            if (cgResult.IsSuccess)
                _katMotionFileService.SaveDefaultConfigGroup(cgResult.Value);
        }
    }
}
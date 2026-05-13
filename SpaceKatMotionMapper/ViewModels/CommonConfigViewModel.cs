using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CSharpFunctionalExtensions;
using SpaceKat.Shared.Services.Contract;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.ViewModels;

public partial class CommonConfigViewModel : ViewModelBase
{
    [ObservableProperty] private KatMotionConfigViewModel _defaultKatMotionConfig;

    private readonly IKatMotionFileService _katMotionFileService;
    private readonly IKatMotionConfigVMManageService _katMotionConfigVmManageService;
    private readonly IKatMotionTimeConfigService _katMotionTimeConfigService;
    private readonly IKatDeadZoneConfigService _katDeadZoneConfigService;
    private readonly IActivationStatusService _activationStatusService;

    public CommonConfigViewModel(
        IKatMotionFileService katMotionFileService,
        IKatMotionConfigVMManageService katMotionConfigVmManageService,
        IKatMotionTimeConfigService katMotionTimeConfigService,
        IKatDeadZoneConfigService katDeadZoneConfigService,
        IActivationStatusService activationStatusService,
        KatMotionConfigViewModel defaultKatMotionConfig)
    {
        _katMotionFileService = katMotionFileService;
        _katMotionConfigVmManageService = katMotionConfigVmManageService;
        _katMotionTimeConfigService = katMotionTimeConfigService;
        _katDeadZoneConfigService = katDeadZoneConfigService;
        _activationStatusService = activationStatusService;
        DefaultKatMotionConfig = defaultKatMotionConfig;
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
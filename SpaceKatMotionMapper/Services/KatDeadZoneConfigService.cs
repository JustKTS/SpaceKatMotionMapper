using System;
using CSharpFunctionalExtensions;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class KatDeadZoneConfigService(
    IKatMotionFileService katMotionFileService,
    KatMotionRecognizeService katMotionRecognizeService,
    IKatMotionConfigVMManageService katMotionConfigVmManageService)
{
    public KatDeadZoneConfig LoadDefaultDeadZoneConfigs()
    {
        var configRet = katMotionConfigVmManageService.GetDefaultConfig().Map(config => config.DeadZoneConfig);
        if (configRet.IsSuccess)
        {
            var config = configRet.Value;
            if (config.AxesInverse is null)
            {
                config = config with { AxesInverse = [false, false, false, false, false, false] };
            }

            return config;
        }
        return new KatDeadZoneConfig();
    }

    public KatDeadZoneConfig? LoadDeadZoneConfigs(Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.IsSuccess ? configRet.Value.DeadZoneConfig with { } : null;
    }

    public Result<bool, Exception> SaveDefaultDeadZoneConfig(KatDeadZoneConfig deadZoneConfig)
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Bind(configVm =>
        {
            try
            {
                configVm.DeadZoneConfig = deadZoneConfig;
                return configVm.ToKatMotionConfigGroups().Bind(katMotionFileService.SaveDefaultConfigGroup);
            }
            catch (Exception e)
            {
                return e;
            }
        });
    }

    public Result<bool, Exception> SaveDeadZoneConfig(KatDeadZoneConfig deadZoneConfig, Guid configGroupId)
    {
        return katMotionConfigVmManageService.GetConfig(configGroupId).Bind(configVm =>
        {
            try
            {
                configVm.IsCustomDeadZone = true;
                configVm.DeadZoneConfig = deadZoneConfig;
                return configVm.ToKatMotionConfigGroups().Bind(katMotionFileService.SaveConfigGroupToSysConf);
            }
            catch (Exception e)
            {
                return e;
            }
        });
    }

    public bool ApplyDeadZoneConfigById(Guid id)
    {
        var result = katMotionConfigVmManageService.GetConfig(id);
        if (result.IsSuccess)
        {
            var vm = result.Value;
            katMotionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower,
                vm.DeadZoneConfig.AxesInverse);
            return true;
        }
        return false;
    }

    public bool ApplyDefaultDeadZoneConfig()
    {
        var result = katMotionConfigVmManageService.GetDefaultConfig();
        if (result.IsSuccess)
        {
            var vm = result.Value;
            katMotionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower,
                vm.DeadZoneConfig.AxesInverse);
            return true;
        }
        return false;
    }
}
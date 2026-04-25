using System;
using LanguageExt;
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
        return configRet.Match(config =>
        {
            // 为保证兼容性添加
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (config.AxesInverse is null)
            {
                config = config with { AxesInverse = [false, false, false, false, false, false] };
            }

            return config;
        }, _ =>new KatDeadZoneConfig());
    }

    public KatDeadZoneConfig? LoadDeadZoneConfigs(Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match<KatDeadZoneConfig?>(config => config.DeadZoneConfig with { }, _ => null);
    }

    public Either<Exception, bool> SaveDefaultDeadZoneConfig(KatDeadZoneConfig deadZoneConfig)
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

    public Either<Exception, bool> SaveDeadZoneConfig(KatDeadZoneConfig deadZoneConfig, Guid configGroupId)
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
        return katMotionConfigVmManageService.GetConfig(id).Match(vm =>
        {
            katMotionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower,
                vm.DeadZoneConfig.AxesInverse);
            return true;
        }, _ => false);
    }

    public bool ApplyDefaultDeadZoneConfig()
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Match(vm =>
        {
            katMotionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower,
                vm.DeadZoneConfig.AxesInverse);
            return true;
        }, _ => false);
    }
}
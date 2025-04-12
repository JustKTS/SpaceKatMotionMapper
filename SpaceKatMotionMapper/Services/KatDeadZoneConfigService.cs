using System;
using System.Diagnostics;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;

namespace SpaceKatMotionMapper.Services;

public class KatDeadZoneConfigService(
    KatMotionFileService katMotionFileService,
    KatMotionRecognizeService katMotionRecognizeService,
    KatMotionConfigVMManageService katMotionConfigVmManageService)
{
    public KatDeadZoneConfig LoadDefaultDeadZoneConfigs()
    {
        var configRet = katMotionConfigVmManageService.GetDefaultConfig();
        var config = configRet.Match<KatDeadZoneConfig?>(config => config.DeadZoneConfig with { }, ex => null);
        if (config is null) return new KatDeadZoneConfig();
        
        // 为保证兼容性添加
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (config.AxesInverse is null)
        {
            config = config with { AxesInverse = [false, false, false, false, false, false]};
        }
        return config;
    }

    public KatDeadZoneConfig? LoadDeadZoneConfigs(Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match<KatDeadZoneConfig?>(config => config.DeadZoneConfig with { }, ex => null);
    }

    public Result<bool> SaveDefaultDeadZoneConfig(KatDeadZoneConfig deadZoneConfig)
    {
        var configRet = katMotionConfigVmManageService.GetDefaultConfig();
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.DeadZoneConfig = deadZoneConfig;
                var newConfigRet = configVm.ToKatMotionConfigGroups();
                return newConfigRet.Match(katMotionFileService.SaveDefaultConfigGroup, ex => new Result<bool>(ex));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }, ex => false);
    }

    public Result<bool> SaveDeadZoneConfig(KatDeadZoneConfig deadZoneConfig, Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.IsCustomDeadZone = true;
                configVm.DeadZoneConfig = deadZoneConfig;
                var newConfigRet = configVm.ToKatMotionConfigGroups();
                return newConfigRet.Match(katMotionFileService.SaveConfigGroupToSysConf, ex => new Result<bool>(ex));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }, ex => false);
    }

    public bool ApplyDeadZoneConfigById(Guid id)
    {
        var vmRet = katMotionConfigVmManageService.GetConfig(id);
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katMotionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower, vm.DeadZoneConfig.AxesInverse));
        return true;
    }

    public bool ApplyDefaultDeadZoneConfig()
    {
        var vmRet = katMotionConfigVmManageService.GetDefaultConfig();
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katMotionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower, vm.DeadZoneConfig.AxesInverse));
        return true;
    }
}
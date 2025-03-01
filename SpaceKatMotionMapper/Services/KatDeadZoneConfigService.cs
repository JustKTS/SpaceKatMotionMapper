using System;
using System.Diagnostics;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;

namespace SpaceKatMotionMapper.Services;

public class KatDeadZoneConfigService(
    KatActionFileService katActionFileService,
    KatActionRecognizeService katActionRecognizeService,
    KatActionConfigVMManageService katActionConfigVmManageService)
{
    public KatDeadZoneConfig LoadDefaultDeadZoneConfigs()
    {
        var configRet = katActionConfigVmManageService.GetDefaultConfig();
        var config = configRet.Match<KatDeadZoneConfig?>(config => config.DeadZoneConfig with {}, ex => null);
        return config ?? new KatDeadZoneConfig();
    }

    public KatDeadZoneConfig? LoadDeadZoneConfigs(Guid configGroupId)
    {
        var configRet = katActionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match<KatDeadZoneConfig?>(config => config.DeadZoneConfig with {}, ex => null);
    }

    public Result<bool> SaveDefaultDeadZoneConfig(KatDeadZoneConfig deadZoneConfig)
    {
        var configRet = katActionConfigVmManageService.GetDefaultConfig();
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.DeadZoneConfig = deadZoneConfig;
                var newConfigRet = configVm.ToKatActionConfigGroups();
                return newConfigRet.Match(katActionFileService.SaveDefaultConfigGroup, ex => new Result<bool>(ex));
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
        var configRet = katActionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.IsCustomDeadZone = true;
                configVm.DeadZoneConfig = deadZoneConfig;
                var newConfigRet = configVm.ToKatActionConfigGroups();
                return newConfigRet.Match(katActionFileService.SaveConfigGroupToSysConf, ex => new Result<bool>(ex));
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
        var vmRet = katActionConfigVmManageService.GetConfig(id);
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katActionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower));
        return true;
    }
    public bool ApplyDefaultDeadZoneConfig()
    {
        var vmRet = katActionConfigVmManageService.GetDefaultConfig();
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katActionRecognizeService.SetDeadZone(vm.DeadZoneConfig.Upper, vm.DeadZoneConfig.Lower));
        return true;
    }
}
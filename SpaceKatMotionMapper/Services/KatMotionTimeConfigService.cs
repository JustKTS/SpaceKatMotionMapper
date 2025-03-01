using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;

namespace SpaceKatMotionMapper.Services;

public class KatMotionTimeConfigService(
    KatActionFileService katActionFileService,
    KatActionConfigVMManageService katActionConfigVmManageService,
    KatActionRecognizeService katActionRecognizeService)
{
    public KatMotionTimeConfigs LoadDefaultTimeConfigs()
    {
        var configRet = katActionConfigVmManageService.GetDefaultConfig();
        var config = configRet.Match<KatMotionTimeConfigs?>(config => config.MotionTimeConfigs with {}, ex => null);
        return config ?? new KatMotionTimeConfigs(true);
    }

    public KatMotionTimeConfigs? LoadMotionTimeConfigs(Guid configGroupId)
    {
        var configRet = katActionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match<KatMotionTimeConfigs?>(config => config.MotionTimeConfigs with {}, ex => null);
    }

    public Result<bool> SaveDefaultTimeConfig(KatMotionTimeConfigs timeConfig)
    {
        var configRet = katActionConfigVmManageService.GetDefaultConfig();
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.MotionTimeConfigs = timeConfig;
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

    public Result<bool> SaveTimeConfig(KatMotionTimeConfigs timeConfig, Guid configGroupId)
    {
        var configRet = katActionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.IsCustomMotionTimeConfigs = true;
                configVm.MotionTimeConfigs = timeConfig;
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
    
    public bool ApplyMotionTimeConfigById(Guid id)
    {
        var vmRet = katActionConfigVmManageService.GetConfig(id);
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katActionRecognizeService.UpdateMotionTimeConfigs(vm.MotionTimeConfigs));
        return true;
    }
    
    public bool ApplyDefaultMotionTimeConfig()
    {
        var vmRet = katActionConfigVmManageService.GetDefaultConfig();
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katActionRecognizeService.UpdateMotionTimeConfigs(vm.MotionTimeConfigs));
        return true;
    }
}
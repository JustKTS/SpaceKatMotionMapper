using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using LanguageExt.Common;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;

namespace SpaceKatMotionMapper.Services;

public class KatMotionTimeConfigService(
    KatMotionFileService katMotionFileService,
    KatMotionConfigVMManageService katMotionConfigVmManageService,
    KatMotionRecognizeService katMotionRecognizeService)
{
    public KatMotionTimeConfigs LoadDefaultTimeConfigs()
    {
        var configRet = katMotionConfigVmManageService.GetDefaultConfig();
        var config = configRet.Match<KatMotionTimeConfigs?>(config => config.MotionTimeConfigs with {}, ex => null);
        return config ?? new KatMotionTimeConfigs();
    }

    public KatMotionTimeConfigs? LoadMotionTimeConfigs(Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match<KatMotionTimeConfigs?>(config => config.MotionTimeConfigs with {}, ex => null);
    }

    public Result<bool> SaveDefaultTimeConfig(KatMotionTimeConfigs timeConfig)
    {
        var configRet = katMotionConfigVmManageService.GetDefaultConfig();
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.MotionTimeConfigs = timeConfig;
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

    public Result<bool> SaveTimeConfig(KatMotionTimeConfigs timeConfig, Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match(configVm =>
        {
            try
            {
                configVm.IsCustomMotionTimeConfigs = true;
                configVm.MotionTimeConfigs = timeConfig;
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
    
    public bool ApplyMotionTimeConfigById(Guid id)
    {
        var vmRet = katMotionConfigVmManageService.GetConfig(id);
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katMotionRecognizeService.UpdateMotionTimeConfigs(vm.MotionTimeConfigs));
        return true;
    }
    
    public bool ApplyDefaultMotionTimeConfig()
    {
        var vmRet = katMotionConfigVmManageService.GetDefaultConfig();
        if (!vmRet.IsSuccess) return false;
        vmRet.IfSucc(vm => katMotionRecognizeService.UpdateMotionTimeConfigs(vm.MotionTimeConfigs));
        return true;
    }
}
using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using LanguageExt;
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

    public Either<Exception, bool> SaveDefaultTimeConfig(KatMotionTimeConfigs timeConfig)
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Bind<bool>(configVm =>
        {
            try
            {
                configVm.MotionTimeConfigs = timeConfig;
                return configVm.ToKatMotionConfigGroups().Bind<bool>(katMotionFileService.SaveDefaultConfigGroup);
            }
            catch (Exception e)
            {
                return e;
            }
        });
    }

    public Either<Exception,bool> SaveTimeConfig(KatMotionTimeConfigs timeConfig, Guid configGroupId)
    {
        return katMotionConfigVmManageService.GetConfig(configGroupId).Bind(
            configVm =>
        {
            try
            {
                configVm.IsCustomMotionTimeConfigs = true;
                configVm.MotionTimeConfigs = timeConfig;
                return configVm.ToKatMotionConfigGroups().Bind(katMotionFileService.SaveConfigGroupToSysConf);
            }
            catch (Exception e)
            {
                return e;
            }
        });
    }
    
    public bool ApplyMotionTimeConfigById(Guid id)
    {
        return katMotionConfigVmManageService.GetConfig(id).Match(vm =>
        {
            katMotionRecognizeService.UpdateMotionTimeConfigs(vm.MotionTimeConfigs);
            return true;
        }, _ => false);
    }
    
    public bool ApplyDefaultMotionTimeConfig()
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Match(vm =>
        {
            katMotionRecognizeService.UpdateMotionTimeConfigs(vm.MotionTimeConfigs);
            return true;
        }, _ => false);
    }
}
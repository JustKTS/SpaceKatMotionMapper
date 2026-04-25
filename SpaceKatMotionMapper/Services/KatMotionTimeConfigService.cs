using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class KatMotionTimeConfigService(
    IKatMotionFileService katMotionFileService,
    IKatMotionConfigVMManageService katMotionConfigVmManageService,
    KatMotionRecognizeService katMotionRecognizeService)
{
    public KatMotionTimeConfigs LoadDefaultTimeConfigs()
    {
        return katMotionConfigVmManageService
            .GetDefaultConfig()
            .Match<KatMotionTimeConfigs>(
                config => GetEffectiveMotionTimeConfigs(config),
                _ => new KatMotionTimeConfigs());
    }

    public KatMotionTimeConfigs? LoadMotionTimeConfigs(Guid configGroupId)
    {
        var configRet = katMotionConfigVmManageService.GetConfig(configGroupId);
        return configRet.Match<KatMotionTimeConfigs?>(config => GetEffectiveMotionTimeConfigs(config), _ => null);
    }

    public Either<Exception, bool> SaveDefaultTimeConfig(KatMotionTimeConfigs timeConfig)
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Bind(configVm =>
        {
            try
            {
                configVm.MotionTimeConfigs = timeConfig;
                return configVm.ToKatMotionConfigGroups().Bind(katMotionFileService.SaveDefaultConfigGroup);
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
            katMotionRecognizeService.UpdateMotionTimeConfigs(GetEffectiveMotionTimeConfigs(vm));
            return true;
        }, _ => false);
    }
    
    public bool ApplyDefaultMotionTimeConfig()
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Match(vm =>
        {
            katMotionRecognizeService.UpdateMotionTimeConfigs(GetEffectiveMotionTimeConfigs(vm));
            return true;
        }, _ => false);
    }

    private static KatMotionTimeConfigs GetEffectiveMotionTimeConfigs(ViewModels.KatMotionConfigViewModel vm)
    {
        try
        {
            return vm.GetEffectiveMotionTimeConfigs();
        }
        catch
        {
            return vm.MotionTimeConfigs;
        }
    }

    public System.Collections.Generic.HashSet<KatMotionEnum> GetSingleActionMotionsFromDefaultConfig()
    {
        return katMotionConfigVmManageService.GetDefaultConfig().Match(
            GetSingleActionMotions,
            _ => new System.Collections.Generic.HashSet<KatMotionEnum>());
    }

    public System.Collections.Generic.HashSet<KatMotionEnum> GetSingleActionMotionsById(Guid configGroupId)
    {
        return katMotionConfigVmManageService.GetConfig(configGroupId).Match(
            GetSingleActionMotions,
            _ => new System.Collections.Generic.HashSet<KatMotionEnum>());
    }

    private static System.Collections.Generic.HashSet<KatMotionEnum> GetSingleActionMotions(ViewModels.KatMotionConfigViewModel vm)
    {
        try
        {
            return vm.KatMotionsWithMode
                .SelectMany(mode => mode.KatMotionGroups.SelectMany(group => group.Configs))
                .Where(config => config.ConfigMode == KatConfigModeEnum.SingleAction && config.KatMotion != KatMotionEnum.Null)
                .Select(config => config.KatMotion)
                .ToHashSet();
        }
        catch
        {
            return new System.Collections.Generic.HashSet<KatMotionEnum>();
        }
    }
}
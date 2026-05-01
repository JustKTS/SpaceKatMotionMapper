using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
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
        var result = katMotionConfigVmManageService.GetDefaultConfig();
        return result.IsSuccess ? GetEffectiveMotionTimeConfigs(result.Value) : new KatMotionTimeConfigs();
    }

    public KatMotionTimeConfigs? LoadMotionTimeConfigs(Guid configGroupId)
    {
        var result = katMotionConfigVmManageService.GetConfig(configGroupId);
        return result.IsSuccess ? GetEffectiveMotionTimeConfigs(result.Value) : null;
    }

    public Result<bool, Exception> SaveDefaultTimeConfig(KatMotionTimeConfigs timeConfig)
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

    public Result<bool, Exception> SaveTimeConfig(KatMotionTimeConfigs timeConfig, Guid configGroupId)
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
        var result = katMotionConfigVmManageService.GetConfig(id);
        if (result.IsSuccess)
        {
            katMotionRecognizeService.UpdateMotionTimeConfigs(GetEffectiveMotionTimeConfigs(result.Value));
            return true;
        }
        return false;
    }

    public bool ApplyDefaultMotionTimeConfig()
    {
        var result = katMotionConfigVmManageService.GetDefaultConfig();
        if (result.IsSuccess)
        {
            katMotionRecognizeService.UpdateMotionTimeConfigs(GetEffectiveMotionTimeConfigs(result.Value));
            return true;
        }
        return false;
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
        var result = katMotionConfigVmManageService.GetDefaultConfig();
        return result.IsSuccess ? GetSingleActionMotions(result.Value) : new System.Collections.Generic.HashSet<KatMotionEnum>();
    }

    public System.Collections.Generic.HashSet<KatMotionEnum> GetSingleActionMotionsById(Guid configGroupId)
    {
        var result = katMotionConfigVmManageService.GetConfig(configGroupId);
        return result.IsSuccess ? GetSingleActionMotions(result.Value) : new System.Collections.Generic.HashSet<KatMotionEnum>();
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
using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Functions.SemanticRules;

public class PressReleaseBalanceSemanticRule : IKatMotionConfigSemanticRule
{
    public Either<Exception, bool> Validate(in KatMotionConfigSemanticValidationContext context)
    {
        if (context.Items.Any(item => item.ConfigMode == KatConfigModeEnum.SingleAction))
        {
            return true;
        }

        var pressKeys = new List<string>();
        var releaseKeys = new List<string>();
        foreach (var item in context.Items)
        {
            pressKeys.AddRange(item.Actions
                .Where(action => action.PressMode == PressModeEnum.Press)
                .Select(action => action.Key));
            releaseKeys.AddRange(item.Actions
                .Where(action => action.PressMode == PressModeEnum.Release)
                .Select(action => action.Key));
        }

        var hasMatchingPresses = pressKeys.All(releaseKeys.Contains);
        var hasMatchingReleases = releaseKeys.All(pressKeys.Contains);
        if (hasMatchingPresses && hasMatchingReleases)
        {
            return true;
        }

        var pressButNotReleaseKeys = pressKeys.Except(releaseKeys).ToArray();
        var releaseButNotPressKeys = releaseKeys.Except(pressKeys).ToArray();
        var exceptionStr = string.Empty;
        if (pressButNotReleaseKeys.Length != 0)
        {
            exceptionStr += $"按键{string.Join(",", pressButNotReleaseKeys)}配置了按下但没有被释放";
        }

        if (releaseButNotPressKeys.Length != 0)
        {
            exceptionStr += $"按键{string.Join(",", releaseButNotPressKeys)}配置了释放但没有被按下";
        }

        return new Exception(exceptionStr);
    }
}



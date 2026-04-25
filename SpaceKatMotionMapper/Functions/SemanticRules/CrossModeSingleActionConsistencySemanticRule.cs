using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Functions.SemanticRules;

public class CrossModeSingleActionConsistencySemanticRule : IKatMotionConfigSemanticRule
{
    public Either<Exception, bool> Validate(in KatMotionConfigSemanticValidationContext context)
    {
        var motionConfigModes = new Dictionary<KatMotionEnum, List<KatConfigModeEnum>>();

        foreach (var item in context.Items)
        {
            if (item.Motion == KatMotionEnum.Null) continue;

            if (!motionConfigModes.TryGetValue(item.Motion, out var value))
            {
                value = [];
                motionConfigModes[item.Motion] = value;
            }

            value.Add(item.ConfigMode);
        }

        foreach (var (motion, modes) in motionConfigModes)
        {
            var hasSingleAction = modes.Contains(KatConfigModeEnum.SingleAction);
            var hasNonSingleAction = modes.Contains(KatConfigModeEnum.Advanced) ||
                                     modes.Contains(KatConfigModeEnum.Expert);

            if (hasSingleAction && hasNonSingleAction)
            {
                return new Exception(
                    $"运动方式 {motion} 在不同模式中的配置模式不一致。如果一个KatMotion被配置为单动作模式，那么在所有模式中都必须配置为单动作模式。");
            }
        }

        return true;
    }
}



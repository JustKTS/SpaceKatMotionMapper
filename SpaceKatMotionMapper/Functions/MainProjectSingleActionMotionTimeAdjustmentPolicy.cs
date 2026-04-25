using System.Collections.Generic;
using System.Linq;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Functions;

public class MainProjectSingleActionMotionTimeAdjustmentPolicy : IKatMotionTimeConfigAdjustmentPolicy
{
    private const double DefaultRepeatScaleFactor = 2;

    public KatMotionTimeConfigs Adjust(
        KatMotionTimeConfigs source,
        IReadOnlyList<MotionTimeAdjustmentInput> inputs)
    {
        var singleActionInputs = inputs
            .Where(input => input.ConfigMode == KatConfigModeEnum.SingleAction && input.Motion != KatMotionEnum.Null)
            .ToList();

        var singleActionMotions = singleActionInputs
            .Select(input => input.Motion)
            .ToHashSet();

        if (singleActionMotions.Count == 0)
        {
            return source;
        }

        var singleActionRepeatPolicies = singleActionInputs
            .GroupBy(input => input.Motion)
            .ToDictionary(group => group.Key, group => group.All(item => item.ShouldEnableAutoRepeat));

        var adjustedConfigs = new Dictionary<KatMotionEnum, KatTriggerTimesConfig>();
        foreach (var kvp in source.Configs)
        {
            if (singleActionMotions.Contains(kvp.Key) && !source.OverriddenMotions.Contains(kvp.Key))
            {
                var shouldEnableAutoRepeat = singleActionRepeatPolicies.GetValueOrDefault(kvp.Key, false);
                var repeatScaleFactor = shouldEnableAutoRepeat
                    ? (kvp.Value.LongReachRepeatScaleFactor > 0
                        ? kvp.Value.LongReachRepeatScaleFactor
                        : DefaultRepeatScaleFactor)
                    : 0.0;

                adjustedConfigs[kvp.Key] = new KatTriggerTimesConfig(
                    kvp.Value.ShortRepeatToleranceMs,
                    source.DefaultSingleActionLongReachTimeoutMs,
                    kvp.Value.LongReachRepeatTimeSpanMs,
                    repeatScaleFactor);
            }
            else if (source.OverriddenMotions.Contains(kvp.Key))
            {
                adjustedConfigs[kvp.Key] = kvp.Value;
            }
            else
            {
                adjustedConfigs[kvp.Key] = new KatTriggerTimesConfig(
                    kvp.Value.ShortRepeatToleranceMs,
                    source.DefaultLongReachTimeoutMs,
                    kvp.Value.LongReachRepeatTimeSpanMs,
                    source.DefaultRepeatScaleFactor);
            }
        }

        return new KatMotionTimeConfigs(adjustedConfigs);
    }
}




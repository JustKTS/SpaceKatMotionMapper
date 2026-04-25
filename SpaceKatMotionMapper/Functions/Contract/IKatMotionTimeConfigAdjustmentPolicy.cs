using System.Collections.Generic;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Functions.Contract;

public readonly record struct MotionTimeAdjustmentInput(
    KatMotionEnum Motion,
    KatConfigModeEnum ConfigMode,
    bool ShouldEnableAutoRepeat);

public interface IKatMotionTimeConfigAdjustmentPolicy
{
    KatMotionTimeConfigs Adjust(
        KatMotionTimeConfigs source,
        IReadOnlyList<MotionTimeAdjustmentInput> inputs);
}



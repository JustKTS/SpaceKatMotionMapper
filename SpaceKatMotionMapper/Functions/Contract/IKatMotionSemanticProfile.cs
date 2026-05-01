using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Functions.Contract;

public interface IKatMotionSemanticProfile
{
    Result<bool, Exception> ValidatePreModeGraph(in KatMotionConfigSemanticValidationContext context);
    Result<bool, Exception> ValidatePostModeGraph(in KatMotionConfigSemanticValidationContext context);
    KatMotionTimeConfigs AdjustMotionTimeConfigs(
        KatMotionTimeConfigs source,
        IReadOnlyList<MotionTimeAdjustmentInput> inputs);
}


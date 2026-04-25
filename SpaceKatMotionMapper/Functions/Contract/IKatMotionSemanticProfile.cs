using System;
using System.Collections.Generic;
using LanguageExt;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Functions.Contract;

public interface IKatMotionSemanticProfile
{
    Either<Exception, bool> ValidatePreModeGraph(in KatMotionConfigSemanticValidationContext context);

    Either<Exception, bool> ValidatePostModeGraph(in KatMotionConfigSemanticValidationContext context);

    KatMotionTimeConfigs AdjustMotionTimeConfigs(
        KatMotionTimeConfigs source,
        IReadOnlyList<MotionTimeAdjustmentInput> inputs);
}



using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Functions;

public class MainProjectKatMotionSemanticProfile(
    MainProjectKatMotionSemanticRuleAssembler? ruleAssembler = null,
    IKatMotionTimeConfigAdjustmentPolicy? timeConfigAdjustmentPolicy = null)
    : IKatMotionSemanticProfile
{
    private readonly MainProjectKatMotionSemanticRuleAssembler _ruleAssembler = ruleAssembler ?? new MainProjectKatMotionSemanticRuleAssembler();
    private readonly IKatMotionTimeConfigAdjustmentPolicy _timeConfigAdjustmentPolicy = timeConfigAdjustmentPolicy ?? new MainProjectSingleActionMotionTimeAdjustmentPolicy();

    public Result<bool, Exception> ValidatePreModeGraph(in KatMotionConfigSemanticValidationContext context)
    {
        return MainProjectKatMotionSemanticRuleAssembler.CreatePreModeGraphValidator().Validate(context);
    }

    public Result<bool, Exception> ValidatePostModeGraph(in KatMotionConfigSemanticValidationContext context)
    {
        return MainProjectKatMotionSemanticRuleAssembler.CreatePostModeGraphValidator().Validate(context);
    }

    public KatMotionTimeConfigs AdjustMotionTimeConfigs(
        KatMotionTimeConfigs source,
        IReadOnlyList<MotionTimeAdjustmentInput> inputs)
    {
        return _timeConfigAdjustmentPolicy.Adjust(source, inputs);
    }
}



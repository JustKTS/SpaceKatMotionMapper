using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Functions;

public class KatMotionConfigSemanticRulePipeline(IReadOnlyList<IKatMotionConfigSemanticRule> rules)
{
    public Result<bool, Exception> Validate(in KatMotionConfigSemanticValidationContext context)
    {
        foreach (var rule in rules)
        {
            var result = rule.Validate(context);
            if (result.IsFailure) return result;
        }

        return true;
    }
}



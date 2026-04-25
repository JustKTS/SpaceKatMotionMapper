using System;
using System.Collections.Generic;
using LanguageExt;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Functions;

public class KatMotionConfigSemanticRulePipeline(IReadOnlyList<IKatMotionConfigSemanticRule> rules)
{
    public Either<Exception, bool> Validate(in KatMotionConfigSemanticValidationContext context)
    {
        foreach (var rule in rules)
        {
            var result = rule.Validate(context);
            if (result.IsLeft) return result;
        }

        return true;
    }
}



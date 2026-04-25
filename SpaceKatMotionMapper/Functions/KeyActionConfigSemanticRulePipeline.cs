using System.Collections.Generic;
using System.Linq;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Functions;

public class KeyActionConfigSemanticRulePipeline(IReadOnlyList<IKeyActionConfigSemanticRule> rules)
    : IKeyActionConfigSemanticValidator
{
    public bool Validate(IReadOnlyList<KeyActionConfig> actions, bool isSingleActionMode)
    {
        var context = new KeyActionSemanticValidationContext(actions, isSingleActionMode);
        return rules.All(rule => rule.Validate(context));
    }
}


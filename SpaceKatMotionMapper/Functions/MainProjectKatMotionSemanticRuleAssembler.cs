using System.Collections.Generic;
using SpaceKatMotionMapper.Functions.SemanticRules;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Functions;

public class MainProjectKatMotionSemanticRuleAssembler
{
    public static KatMotionConfigSemanticRulePipeline CreatePreModeGraphValidator()
    {
        return new KatMotionConfigSemanticRulePipeline(new List<IKatMotionConfigSemanticRule>
        {
            new PressReleaseBalanceSemanticRule()
        });
    }

    public static KatMotionConfigSemanticRulePipeline CreatePostModeGraphValidator()
    {
        return new KatMotionConfigSemanticRulePipeline(new List<IKatMotionConfigSemanticRule>
        {
            new CrossModeSingleActionConsistencySemanticRule()
        });
    }
}




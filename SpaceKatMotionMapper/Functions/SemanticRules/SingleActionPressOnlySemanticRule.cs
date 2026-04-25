using System.Linq;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Functions.SemanticRules;

public class SingleActionPressOnlySemanticRule : IKeyActionConfigSemanticRule
{
    public bool Validate(in KeyActionSemanticValidationContext context)
    {
        if (!context.IsSingleActionMode) return true;

        return context.Actions
            .Where(action => action.ActionType is ActionType.KeyBoard or ActionType.Mouse)
            .All(action => action.PressMode == PressModeEnum.Press);
    }
}


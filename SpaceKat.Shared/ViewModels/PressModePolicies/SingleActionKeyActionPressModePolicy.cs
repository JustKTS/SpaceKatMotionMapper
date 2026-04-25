using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.ViewModels.PressModePolicies;

public class SingleActionKeyActionPressModePolicy : IKeyActionPressModePolicy
{
    public PressModeEnum GetDefaultPressMode(ActionType actionType)
    {
        return actionType is ActionType.KeyBoard or ActionType.Mouse
            ? PressModeEnum.Press
            : PressModeEnum.None;
    }

    public PressModeEnum CoercePressMode(ActionType actionType, PressModeEnum requestedPressMode)
    {
        return actionType is ActionType.KeyBoard or ActionType.Mouse
            ? PressModeEnum.Press
            : requestedPressMode;
    }
}


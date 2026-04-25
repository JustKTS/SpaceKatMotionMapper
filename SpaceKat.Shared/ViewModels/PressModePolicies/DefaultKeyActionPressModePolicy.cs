using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.ViewModels.PressModePolicies;

public class DefaultKeyActionPressModePolicy : IKeyActionPressModePolicy
{
    public PressModeEnum GetDefaultPressMode(ActionType actionType)
    {
        return PressModeEnum.None;
    }

    public PressModeEnum CoercePressMode(ActionType actionType, PressModeEnum requestedPressMode)
    {
        return requestedPressMode;
    }
}


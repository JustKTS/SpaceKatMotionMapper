using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.ViewModels.PressModePolicies;

public interface IKeyActionPressModePolicy
{
    PressModeEnum GetDefaultPressMode(ActionType actionType);

    PressModeEnum CoercePressMode(ActionType actionType, PressModeEnum requestedPressMode);
}


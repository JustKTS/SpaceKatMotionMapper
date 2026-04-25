using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Functions;

public class KeyActionAvailabilityValidator : IKeyActionAvailabilityValidator
{
    public bool Validate(
        ActionType actionType,
        string key,
        PressModeEnum pressMode,
        int multiplier,
        KeyActionAvailabilityValidationOptions options)
    {
        switch (actionType)
        {
            case ActionType.KeyBoard:
                return key != KeyActionConstants.NoneKeyValue && pressMode != PressModeEnum.None;
            case ActionType.Mouse:
                if (key == KeyActionConstants.NoneKeyValue) return false;
                return ValidateMouse(key, pressMode, multiplier, options);
            case ActionType.Delay:
                return multiplier >= KeyActionConstants.MinDelayMultiplier;
            case ActionType.None:
            default:
                return false;
        }
    }

    private static bool ValidateMouse(
        string key,
        PressModeEnum pressMode,
        int multiplier,
        KeyActionAvailabilityValidationOptions options)
    {
        try
        {
            var mouseButton = MouseButtonEnum.Parse(key, ignoreCase: true, allowMatchingMetadataAttribute: true);
            return mouseButton switch
            {
                MouseButtonEnum.ScrollUp or MouseButtonEnum.ScrollDown => options.RequirePositiveScrollMultiplier
                    ? multiplier > 0
                    : multiplier != 0,
                _ => pressMode != PressModeEnum.None
            };
        }
        catch
        {
            return false;
        }
    }
}


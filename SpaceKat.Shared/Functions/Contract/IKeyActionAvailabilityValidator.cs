using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Functions.Contract;

public readonly record struct KeyActionAvailabilityValidationOptions(bool RequirePositiveScrollMultiplier);

public interface IKeyActionAvailabilityValidator
{
    bool Validate(
        ActionType actionType,
        string key,
        PressModeEnum pressMode,
        int multiplier,
        KeyActionAvailabilityValidationOptions options);
}


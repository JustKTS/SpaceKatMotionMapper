using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.Services.Contract;

public interface IKeyActionConfigStrategyProfileBase
{
    IHotKeyActionExpansionService HotKeyActionExpansionService { get; }

    IKeyActionAvailabilityValidator AvailabilityValidator { get; }

    IKeyActionPressModePolicy DefaultPressModePolicy { get; }

    IKeyActionPressModePolicy GetPressModePolicy(bool isSingleActionMode)
    {
        return DefaultPressModePolicy;
    }
}


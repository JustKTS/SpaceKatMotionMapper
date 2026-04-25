using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.Services;

public class DefaultSharedKeyActionConfigStrategyProfile : ISharedKeyActionConfigStrategyProfile
{
    public IHotKeyActionExpansionService HotKeyActionExpansionService { get; } = new HotKeyActionExpansionService();

    public IKeyActionPressModePolicy PressModePolicy { get; } = new DefaultKeyActionPressModePolicy();

    public IKeyActionPressModePolicy DefaultPressModePolicy => PressModePolicy;

    public IKeyActionAvailabilityValidator AvailabilityValidator { get; } = new KeyActionAvailabilityValidator();

    public IKeyActionPressModePolicy GetPressModePolicy(bool isSingleActionMode)
    {
        return PressModePolicy;
    }
}


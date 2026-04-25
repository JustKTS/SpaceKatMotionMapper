using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.Services.Contract;

public interface ISharedKeyActionConfigStrategyProfile : IKeyActionConfigStrategyProfileBase
{
    IKeyActionPressModePolicy PressModePolicy { get; }

    IKeyActionPressModePolicy IKeyActionConfigStrategyProfileBase.DefaultPressModePolicy => PressModePolicy;
}



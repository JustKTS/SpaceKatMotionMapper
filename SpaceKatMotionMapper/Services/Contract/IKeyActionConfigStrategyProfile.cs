using System.Collections.Generic;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels.PressModePolicies;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKeyActionConfigStrategyProfile : IKeyActionConfigStrategyProfileBase
{
    IReadOnlyList<IKeyActionConfigSemanticValidator> SemanticValidators { get; }

    IKeyActionPressModePolicy IKeyActionConfigStrategyProfileBase.DefaultPressModePolicy => GetPressModePolicy(false);

    new IKeyActionPressModePolicy GetPressModePolicy(bool isSingleActionMode);
}


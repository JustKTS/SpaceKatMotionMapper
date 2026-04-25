using System.Collections.Generic;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels.PressModePolicies;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class DefaultKeyActionConfigStrategyProfile : IKeyActionConfigStrategyProfile
{
    private readonly IKeyActionPressModePolicy _singleActionPressModePolicy = new SingleActionKeyActionPressModePolicy();

    public IHotKeyActionExpansionService HotKeyActionExpansionService { get; } = new HotKeyActionExpansionService();

    public IKeyActionAvailabilityValidator AvailabilityValidator { get; } = new KeyActionAvailabilityValidator();

    public IReadOnlyList<IKeyActionConfigSemanticValidator> SemanticValidators { get; } =
        [new KeyActionConfigSemanticRulePipeline([])];

    public IKeyActionPressModePolicy DefaultPressModePolicy { get; } = new DefaultKeyActionPressModePolicy();

    public IKeyActionPressModePolicy GetPressModePolicy(bool isSingleActionMode)
    {
        return isSingleActionMode ? _singleActionPressModePolicy : DefaultPressModePolicy;
    }
}



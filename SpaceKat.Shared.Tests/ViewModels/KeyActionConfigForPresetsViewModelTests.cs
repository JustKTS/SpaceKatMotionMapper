using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.Tests.ViewModels;

public class KeyActionConfigForPresetsViewModelTests
{
    [Test]
    public async Task Constructor_WhenProfileUsesSingleActionPolicy_ShouldApplyPressToInitialItem()
    {
        var sut = new KeyActionConfigForPresetsViewModel(new TestSharedStrategyProfile(
            pressModePolicy: new SingleActionKeyActionPressModePolicy()));

        var first = sut.ActionConfigGroups.Single();

        await Assert.That(first.PressMode).IsEqualTo(PressModeEnum.Press);
    }

    [Test]
    public async Task FromKeyActionConfig_ShouldRoundTripUsingBaseCoreMethods()
    {
        var sut = new KeyActionConfigForPresetsViewModel();
        var actions = new[]
        {
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1),
            new KeyActionConfig(ActionType.Delay, KeyActionConstants.NoneKeyValue, PressModeEnum.None, KeyActionConstants.MinDelayMultiplier)
        };

        var loaded = sut.FromKeyActionConfig(actions);
        var roundTripped = sut.ToKeyActionConfigList();

        await Assert.That(loaded).IsTrue();
        await Assert.That(roundTripped.Count).IsEqualTo(2);
        await Assert.That(roundTripped[0]).IsEqualTo(actions[0]);
        await Assert.That(roundTripped[1]).IsEqualTo(actions[1]);
    }

    private sealed class TestSharedStrategyProfile(
        IHotKeyActionExpansionService? hotKeyActionExpansionService = null,
        IKeyActionPressModePolicy? pressModePolicy = null,
        IKeyActionAvailabilityValidator? availabilityValidator = null) : ISharedKeyActionConfigStrategyProfile
    {
        public IHotKeyActionExpansionService HotKeyActionExpansionService { get; } =
            hotKeyActionExpansionService ?? new FixedHotKeyExpansionService([]);

        public IKeyActionPressModePolicy PressModePolicy { get; } =
            pressModePolicy ?? new DefaultKeyActionPressModePolicy();

        public IKeyActionAvailabilityValidator AvailabilityValidator { get; } =
            availabilityValidator ?? new KeyActionAvailabilityValidator();
    }

    private sealed class FixedHotKeyExpansionService(IReadOnlyList<KeyActionConfig> actions) : IHotKeyActionExpansionService
    {
        public IReadOnlyList<KeyActionConfig> Expand(CombinationKeysRecord combinationKeys, bool isSingleActionMode)
        {
            return actions;
        }
    }
}



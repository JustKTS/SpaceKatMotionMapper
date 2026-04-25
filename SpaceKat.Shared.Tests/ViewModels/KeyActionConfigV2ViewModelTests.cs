using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.Tests.ViewModels;

public class KeyActionConfigV2ViewModelTests
{
    [Test]
    public async Task AddHotKeyActions_WithDefaultProfile_ShouldKeepExistingExpansionBehavior()
    {
        var sut = new KeyActionConfigV2ViewModel();

        sut.AddHotKeyActions(useCtrl: true, useWin: false, useAlt: true, useShift: false,
            hotKey: KeyCodeWrapper.A, customDescription: "Ctrl+Alt+A");

        await Assert.That(sut.IsCustomDescription).IsTrue();
        await Assert.That(sut.KeyActionsDescription).IsEqualTo("Ctrl+Alt+A");
        await Assert.That(sut.ToKeyActionConfigList().Count).IsEqualTo(5);
        await Assert.That(sut.ToKeyActionConfigList()[0]).IsEqualTo(
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1));
        await Assert.That(sut.ToKeyActionConfigList()[2]).IsEqualTo(
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1));
        await Assert.That(sut.ToKeyActionConfigList()[4]).IsEqualTo(
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Release, 1));
    }

    [Test]
    public async Task AddHotKeyActions_WhenProfileOverridesExpansion_ShouldUseProfileHotKeyService()
    {
        var sut = new KeyActionConfigV2ViewModel(strategyProfile: new TestSharedStrategyProfile(
            hotKeyActionExpansionService: new FixedHotKeyExpansionService([
                new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.B.GetWrappedName(), PressModeEnum.DoubleClick, 2)
            ])));

        sut.AddHotKeyActions(false, false, false, false, KeyCodeWrapper.A);

        await Assert.That(sut.ToKeyActionConfigList().Count).IsEqualTo(1);
        await Assert.That(sut.ToKeyActionConfigList()[0]).IsEqualTo(
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.B.GetWrappedName(), PressModeEnum.DoubleClick, 2));
    }

    private sealed class FixedHotKeyExpansionService(IReadOnlyList<KeyActionConfig> actions) : IHotKeyActionExpansionService
    {
        public IReadOnlyList<KeyActionConfig> Expand(CombinationKeysRecord combinationKeys, bool isSingleActionMode)
        {
            return actions;
        }
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
}



using FluentAssertions;
using System.Linq;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels.PressModePolicies;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Functions.SemanticRules;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Tests.Helpers;

namespace SpaceKatMotionMapper.Tests.ViewModels;

public class KeyActionConfigViewModelTests
{
    [Test]
    public void AddHotKeyActions_InAdvancedMode_ShouldGeneratePressClickReleaseSequence()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.Advanced);
        var configVm = motionVm.KeyActionConfigGroup;

        configVm.AddHotKeyActions(
            useCtrl: true,
            useWin: false,
            useAlt: true,
            useShift: false,
            hotKey: KeyCodeWrapper.A,
            customDescription: "Ctrl+Alt+A");

        configVm.IsCustomDescription.Should().BeTrue();
        configVm.KeyActionsDescription.Should().Be("Ctrl+Alt+A");
        configVm.IsAvailable.Should().BeTrue();
        var expected = new[]
        {
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.ALT.GetWrappedName(), PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.ALT.GetWrappedName(), PressModeEnum.Release, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Release, 1)
        };

        configVm.ToKeyActionConfigList().SequenceEqual(expected).Should().BeTrue();
    }

    [Test]
    public void AddHotKeyActions_InSingleActionMode_ShouldGeneratePressOnlySequence()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.SingleAction);
        var configVm = motionVm.KeyActionConfigGroup;

        configVm.AddHotKeyActions(
            useCtrl: true,
            useWin: false,
            useAlt: true,
            useShift: false,
            hotKey: KeyCodeWrapper.A,
            customDescription: "Ctrl+Alt+A");

        configVm.IsCustomDescription.Should().BeTrue();
        configVm.KeyActionsDescription.Should().Be("Ctrl+Alt+A");
        configVm.IsAvailable.Should().BeTrue();
        var expected = new[]
        {
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.ALT.GetWrappedName(), PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Press, 1)
        };

        configVm.ToKeyActionConfigList().SequenceEqual(expected).Should().BeTrue();
    }

    [Test]
    public void ToKeyActionConfigList_And_FromKeyActionConfig_ShouldRoundTripWithoutChangingOrder()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.Advanced);
        var configVm = motionVm.KeyActionConfigGroup;
        KeyActionConfig[] expected =
        {
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1),
            new KeyActionConfig(ActionType.Delay, KeyActionConstants.NoneKeyValue, PressModeEnum.None, KeyActionConstants.MinDelayMultiplier),
            new KeyActionConfig(ActionType.Mouse, "LButton", PressModeEnum.Click, 1)
        };

        var loaded = configVm.FromKeyActionConfig(expected);
        var actual = configVm.ToKeyActionConfigList();

        loaded.Should().BeTrue();
        configVm.IsAvailable.Should().BeTrue();
        actual.SequenceEqual(expected).Should().BeTrue();
    }

    [Test]
    public void IsAvailable_WhenSemanticValidatorReturnsFalse_ShouldBeFalse()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.Advanced);
        var configVm = new KeyActionConfigViewModel(
            motionVm,
            semanticValidators: [new AlwaysFalseSemanticValidator()]);
        motionVm.KeyActionConfigGroup = configVm;

        var loaded = configVm.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1)
        ]);

        loaded.Should().BeTrue();
        configVm.ActionConfigGroups.All(a => a.IsAvailable).Should().BeTrue();
        configVm.IsAvailable.Should().BeFalse();
    }

    [Test]
    public void IsAvailable_WhenSingleActionPressOnlyRuleInjected_ShouldRejectClickInSingleActionMode()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.SingleAction);
        var pipeline = new KeyActionConfigSemanticRulePipeline([new SingleActionPressOnlySemanticRule()]);
        var configVm = new KeyActionConfigViewModel(
            motionVm,
            semanticValidators: [pipeline]);
        motionVm.KeyActionConfigGroup = configVm;

        var loaded = configVm.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1)
        ]);

        loaded.Should().BeTrue();
        configVm.ActionConfigGroups.All(a => a.IsAvailable).Should().BeTrue();
        configVm.IsAvailable.Should().BeFalse();
    }

    [Test]
    public void AddHotKeyActions_WhenStrategyProfileOverridesExpansion_ShouldUseProfileServices()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.Advanced);
        var profile = new TestStrategyProfile(
            hotKeyActionExpansionService: new FixedHotKeyExpansionService([
                new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.B.GetWrappedName(), PressModeEnum.DoubleClick, 2)
            ]));
        var configVm = new KeyActionConfigViewModel(motionVm, strategyProfile: profile);
        motionVm.KeyActionConfigGroup = configVm;

        configVm.AddHotKeyActions(false, false, false, false, KeyCodeWrapper.A, "Custom");

        configVm.ToKeyActionConfigList().Should().Equal(
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.B.GetWrappedName(), PressModeEnum.DoubleClick, 2));
    }

    [Test]
    public void IsAvailable_WhenStrategyProfileProvidesSemanticValidator_ShouldRespectProfileValidators()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: SpaceKatMotionMapper.Models.KatConfigModeEnum.Advanced);
        var profile = new TestStrategyProfile(semanticValidators: [new AlwaysFalseSemanticValidator()]);
        var configVm = new KeyActionConfigViewModel(motionVm, strategyProfile: profile);
        motionVm.KeyActionConfigGroup = configVm;

        var loaded = configVm.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1)
        ]);

        loaded.Should().BeTrue();
        configVm.ActionConfigGroups.All(a => a.IsAvailable).Should().BeTrue();
        configVm.IsAvailable.Should().BeFalse();
    }

    private sealed class AlwaysFalseSemanticValidator : IKeyActionConfigSemanticValidator
    {
        public bool Validate(IReadOnlyList<KeyActionConfig> actions, bool isSingleActionMode)
        {
            return false;
        }
    }

    private sealed class FixedHotKeyExpansionService(IReadOnlyList<KeyActionConfig> actions) : IHotKeyActionExpansionService
    {
        public IReadOnlyList<KeyActionConfig> Expand(CombinationKeysRecord combinationKeys, bool isSingleActionMode)
        {
            return actions;
        }
    }

    private sealed class TestStrategyProfile(
        IHotKeyActionExpansionService? hotKeyActionExpansionService = null,
        IKeyActionAvailabilityValidator? availabilityValidator = null,
        IReadOnlyList<IKeyActionConfigSemanticValidator>? semanticValidators = null) : IKeyActionConfigStrategyProfile
    {
        private readonly IKeyActionPressModePolicy _defaultPolicy = new DefaultKeyActionPressModePolicy();
        private readonly IKeyActionPressModePolicy _singleActionPolicy = new SingleActionKeyActionPressModePolicy();

        public IHotKeyActionExpansionService HotKeyActionExpansionService { get; } =
            hotKeyActionExpansionService ?? new FixedHotKeyExpansionService([]);

        public IKeyActionAvailabilityValidator AvailabilityValidator { get; } =
            availabilityValidator ?? new PassThroughAvailabilityValidator();

        public IReadOnlyList<IKeyActionConfigSemanticValidator> SemanticValidators { get; } =
            semanticValidators ?? [];

        public IKeyActionPressModePolicy GetPressModePolicy(bool isSingleActionMode)
        {
            return isSingleActionMode ? _singleActionPolicy : _defaultPolicy;
        }
    }

    private sealed class PassThroughAvailabilityValidator : IKeyActionAvailabilityValidator
    {
        public bool Validate(ActionType actionType, string key, PressModeEnum pressMode, int multiplier,
            KeyActionAvailabilityValidationOptions options)
        {
            return true;
        }
    }
}


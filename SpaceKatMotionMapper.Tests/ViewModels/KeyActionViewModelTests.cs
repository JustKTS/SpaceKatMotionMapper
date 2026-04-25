using TUnit.Assertions;
using TUnit.Core;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Tests.Helpers;

namespace SpaceKatMotionMapper.Tests.ViewModels;

public class KeyActionViewModelTests
{
    [Test]
    public async Task SwitchingActionType_InAdvancedMode_ShouldClearKeyAndRequireNewPressMode()
    {
        // Arrange
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: KatConfigModeEnum.Advanced);
        var actionVm = motionVm.KeyActionConfigGroup.ActionConfigGroups.Single();
        actionVm.Key = "A";
        actionVm.PressMode = PressModeEnum.Click;

        // Act
        actionVm.ActionType = ActionType.Mouse;

        // Assert
        await Assert.That(actionVm.Key).IsEqualTo(KeyActionConstants.NoneKeyValue);
        await Assert.That(actionVm.PressMode).IsEqualTo(PressModeEnum.None);
        await Assert.That(actionVm.IsAvailable).IsFalse();
    }

    [Test]
    public async Task SwitchingActionType_InSingleActionMode_ShouldKeepPressModeAsPress()
    {
        // Arrange
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: KatConfigModeEnum.SingleAction);
        var actionVm = motionVm.KeyActionConfigGroup.ActionConfigGroups.Single();
        actionVm.Key = "A";

        // Act
        actionVm.ActionType = ActionType.Mouse;

        // Assert
        await Assert.That(actionVm.Key).IsEqualTo(KeyActionConstants.NoneKeyValue);
        await Assert.That(actionVm.PressMode).IsEqualTo(PressModeEnum.Press);
    }

    [Test]
    public async Task SwitchingToDelay_ShouldResetInputStateAndClampDelayMultiplier()
    {
        // Arrange
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: KatConfigModeEnum.Advanced);
        var actionVm = motionVm.KeyActionConfigGroup.ActionConfigGroups.Single();
        actionVm.Key = "A";
        actionVm.PressMode = PressModeEnum.Click;
        actionVm.Multiplier = 1;

        // Act
        actionVm.ActionType = ActionType.Delay;

        // Assert
        await Assert.That(actionVm.Key).IsEqualTo(KeyActionConstants.NoneKeyValue);
        await Assert.That(actionVm.PressMode).IsEqualTo(PressModeEnum.None);
        await Assert.That(actionVm.Multiplier).IsEqualTo(KeyActionConstants.MinDelayMultiplier);
        await Assert.That(actionVm.IsAvailable).IsTrue();
    }

    [Test]
    public async Task ScrollMouse_WithNegativeMultiplier_InMainViewModel_ShouldBeUnavailable()
    {
        var motionVm = ViewModelTestHelpers.CreateLeafViewModel(mode: KatConfigModeEnum.Advanced);
        var actionVm = motionVm.KeyActionConfigGroup.ActionConfigGroups.Single();

        actionVm.ActionType = ActionType.Mouse;
        actionVm.Key = nameof(MouseButtonEnum.ScrollUp);
        actionVm.PressMode = PressModeEnum.None;
        actionVm.Multiplier = -1;

        await Assert.That(actionVm.IsAvailable).IsFalse();
    }
}



using SpaceKat.Shared.Models;
using SpaceKat.Shared.ViewModels;

namespace SpaceKat.Shared.Tests.ViewModels;

public class KeyActionWithCommandViewModelTests
{
    [Test]
    public async Task SwitchingActionType_ShouldClearKeyAndRequireNewPressMode()
    {
        var viewModel = new KeyActionWithCommandViewModel(ActionType.KeyBoard, "A", PressModeEnum.Click, 1);

        viewModel.ActionType = ActionType.Mouse;

        await Assert.That(viewModel.Key).IsEqualTo(KeyActionConstants.NoneKeyValue);
        await Assert.That(viewModel.PressMode).IsEqualTo(PressModeEnum.None);
        await Assert.That(viewModel.IsAvailable).IsFalse();
    }

    [Test]
    public async Task SwitchingToDelay_ShouldResetInputStateAndClampDelayMultiplier()
    {
        var viewModel = new KeyActionWithCommandViewModel(ActionType.KeyBoard, "A", PressModeEnum.Click, 1);

        viewModel.ActionType = ActionType.Delay;

        await Assert.That(viewModel.Key).IsEqualTo(KeyActionConstants.NoneKeyValue);
        await Assert.That(viewModel.PressMode).IsEqualTo(PressModeEnum.None);
        await Assert.That(viewModel.Multiplier).IsEqualTo(KeyActionConstants.MinDelayMultiplier);
        await Assert.That(viewModel.IsAvailable).IsTrue();
    }

    [Test]
    public async Task ScrollMouse_WithNegativeMultiplier_InSharedViewModel_ShouldBeAvailable()
    {
        var viewModel = new KeyActionWithCommandViewModel(ActionType.Mouse,
            nameof(MouseButtonEnum.ScrollUp), PressModeEnum.None, -1);

        await Assert.That(viewModel.IsAvailable).IsTrue();
    }
}



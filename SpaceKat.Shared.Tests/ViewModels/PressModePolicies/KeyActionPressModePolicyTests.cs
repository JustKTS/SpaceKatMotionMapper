using TUnit.Assertions;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.Tests.ViewModels.PressModePolicies;

public class KeyActionPressModePolicyTests
{
    [Test]
    public async Task DefaultPolicy_ShouldKeepDefaultAndRequestedPressMode()
    {
        var sut = new DefaultKeyActionPressModePolicy();

        await Assert.That(sut.GetDefaultPressMode(ActionType.KeyBoard)).IsEqualTo(PressModeEnum.None);
        await Assert.That(sut.CoercePressMode(ActionType.KeyBoard, PressModeEnum.Click)).IsEqualTo(PressModeEnum.Click);
        await Assert.That(sut.CoercePressMode(ActionType.Delay, PressModeEnum.None)).IsEqualTo(PressModeEnum.None);
    }

    [Test]
    public async Task SingleActionPolicy_ShouldForceKeyboardAndMouseToPress()
    {
        var sut = new SingleActionKeyActionPressModePolicy();

        await Assert.That(sut.GetDefaultPressMode(ActionType.KeyBoard)).IsEqualTo(PressModeEnum.Press);
        await Assert.That(sut.GetDefaultPressMode(ActionType.Mouse)).IsEqualTo(PressModeEnum.Press);
        await Assert.That(sut.GetDefaultPressMode(ActionType.Delay)).IsEqualTo(PressModeEnum.None);
        await Assert.That(sut.CoercePressMode(ActionType.KeyBoard, PressModeEnum.Click)).IsEqualTo(PressModeEnum.Press);
        await Assert.That(sut.CoercePressMode(ActionType.Mouse, PressModeEnum.None)).IsEqualTo(PressModeEnum.Press);
        await Assert.That(sut.CoercePressMode(ActionType.Delay, PressModeEnum.None)).IsEqualTo(PressModeEnum.None);
    }

    [Test]
    public async Task KeyActionWithCommandViewModel_WhenUsingSingleActionPolicy_ShouldApplyPressDefaults()
    {
        var sut = new KeyActionWithCommandViewModel(ActionType.KeyBoard, "A", PressModeEnum.Click, 1,
            new SingleActionKeyActionPressModePolicy());

        await Assert.That(sut.PressMode).IsEqualTo(PressModeEnum.Press);

        sut.ActionType = ActionType.Mouse;

        await Assert.That(sut.PressMode).IsEqualTo(PressModeEnum.Press);
    }
}



using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;

namespace SpaceKat.Shared.Tests.Services;

public class HotKeyActionExpansionServiceTests
{
    [Test]
    public async Task Expand_WhenAdvancedMode_ShouldEmitPressClickReleaseSequence()
    {
        var sut = new HotKeyActionExpansionService();
        var record = new CombinationKeysRecord(true, false, true, false, KeyCodeWrapper.A);

        var actions = sut.Expand(record, isSingleActionMode: false);

        await Assert.That(actions.Count).IsEqualTo(5);
        await Assert.That(actions[0]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1));
        await Assert.That(actions[1]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.ALT.GetWrappedName(), PressModeEnum.Press, 1));
        await Assert.That(actions[2]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1));
        await Assert.That(actions[3]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.ALT.GetWrappedName(), PressModeEnum.Release, 1));
        await Assert.That(actions[4]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Release, 1));
    }

    [Test]
    public async Task Expand_WhenSingleActionMode_ShouldNotEmitReleaseSequence()
    {
        var sut = new HotKeyActionExpansionService();
        var record = new CombinationKeysRecord(true, true, false, false, KeyCodeWrapper.A);

        var actions = sut.Expand(record, isSingleActionMode: true);

        await Assert.That(actions.Count).IsEqualTo(3);
        await Assert.That(actions[0]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1));
        await Assert.That(actions[1]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.SHIFT.GetWrappedName(), PressModeEnum.Press, 1));
        await Assert.That(actions[2]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Press, 1));
    }

    [Test]
    public async Task Expand_WhenNoModifiers_ShouldEmitMainKeyOnly()
    {
        var sut = new HotKeyActionExpansionService();
        var record = new CombinationKeysRecord(false, false, false, false, KeyCodeWrapper.A);

        var actions = sut.Expand(record, isSingleActionMode: false);

        await Assert.That(actions.Count).IsEqualTo(1);
        await Assert.That(actions[0]).IsEqualTo(new KeyActionConfig(ActionType.KeyBoard,
            KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1));
    }
}


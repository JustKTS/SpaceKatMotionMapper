using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class HotKeyActionExpansionService : IHotKeyActionExpansionService
{
    public IReadOnlyList<KeyActionConfig> Expand(CombinationKeysRecord combinationKeys, bool isSingleActionMode)
    {
        List<KeyActionConfig> actions = [];

        AddModifierIfEnabled(actions, combinationKeys.UseCtrl, KeyCodeWrapper.CONTROL, PressModeEnum.Press);
        AddModifierIfEnabled(actions, combinationKeys.UseWin, KeyCodeWrapper.LWIN, PressModeEnum.Press);
        AddModifierIfEnabled(actions, combinationKeys.UseAlt, KeyCodeWrapper.ALT, PressModeEnum.Press);
        AddModifierIfEnabled(actions, combinationKeys.UseShift, KeyCodeWrapper.SHIFT, PressModeEnum.Press);

        var mainKeyPressMode = isSingleActionMode ? PressModeEnum.Press : PressModeEnum.Click;
        actions.Add(new KeyActionConfig(ActionType.KeyBoard, combinationKeys.Key.GetWrappedName(), mainKeyPressMode, 1));

        if (isSingleActionMode) return actions;

        AddModifierIfEnabled(actions, combinationKeys.UseShift, KeyCodeWrapper.SHIFT, PressModeEnum.Release);
        AddModifierIfEnabled(actions, combinationKeys.UseAlt, KeyCodeWrapper.ALT, PressModeEnum.Release);
        AddModifierIfEnabled(actions, combinationKeys.UseWin, KeyCodeWrapper.LWIN, PressModeEnum.Release);
        AddModifierIfEnabled(actions, combinationKeys.UseCtrl, KeyCodeWrapper.CONTROL, PressModeEnum.Release);

        return actions;
    }

    private static void AddModifierIfEnabled(
        ICollection<KeyActionConfig> actions,
        bool isEnabled,
        KeyCodeWrapper key,
        PressModeEnum pressMode)
    {
        if (!isEnabled) return;
        actions.Add(new KeyActionConfig(ActionType.KeyBoard, key.GetWrappedName(), pressMode, 1));
    }
}


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Helpers;

namespace SpaceKatMotionMapper.Models;

public record KatActionConfig(
    KatAction Action,
    List<KeyActionConfig> ActionConfigs,
    int ModeNum = 0,
    int ToModeNum = 0)
{
    public KatActionConfig() : this(
        new KatAction(KatMotionEnum.Null,
            KatPressModeEnum.Short,
            0),
        [])
    {
    }
}

public record KeyActionConfig(ActionType ActionType, string Key, PressModeEnum PressMode, int Multiplier)
{
    public KeyActionConfig() : this(ActionType.KeyBoard, "None", PressModeEnum.None, 1)
    {
    }

    public bool TryToKeyBoardActionConfig([NotNullWhen(true)] out KeyBoardActionConfig? keyActionConfig)
    {
        keyActionConfig = null;
        if (ActionType is not ActionType.KeyBoard) return false;
        keyActionConfig = new KeyBoardActionConfig(VirtualKeyHelper.Parse(Key), PressMode);
        return true;
    }

    public bool TryToMouseActionConfig([NotNullWhen(true)] out MouseActionConfig? mouseActionConfig)
    {
        mouseActionConfig = null;
        if (ActionType is not ActionType.Mouse) return false;
        mouseActionConfig = new MouseActionConfig(MouseButtonHelper.Parse(Key), PressMode, Multiplier);
        return true;
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SpaceKat.Shared.Helpers;

namespace SpaceKat.Shared.Models;


public record KeyActionConfig(ActionType ActionType, string Key, PressModeEnum PressMode, int Multiplier)
{
    public KeyActionConfig() : this(ActionType.KeyBoard, "None", PressModeEnum.None, 1)
    {
    }

    public bool TryToKeyBoardActionConfig([NotNullWhen(true)] out KeyBoardActionConfig? keyActionConfig)
    {
        keyActionConfig = null;
        if (ActionType is not ActionType.KeyBoard) return false;
        keyActionConfig = new KeyBoardActionConfig(VirtualKeyHelpers.Parse(Key), PressMode);
        return true;
    }

    public bool TryToMouseActionConfig([NotNullWhen(true)] out MouseActionConfig? mouseActionConfig)
    {
        mouseActionConfig = null;
        if (ActionType is not ActionType.Mouse) return false;
        mouseActionConfig = new MouseActionConfig(MouseButtonHelper.Parse(Key), PressMode, Multiplier);
        return true;
    }
    
    public bool TryToDelayActionConfig([NotNullWhen(true)] out DelayActionConfig? delayActionConfig)
    {
        delayActionConfig = null;
        if (ActionType is not ActionType.Delay) return false;
        delayActionConfig = new DelayActionConfig(Multiplier);
        return true;
    }
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KeyActionConfig))]
[JsonSerializable(typeof(ActionType))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(PressModeEnum))]
[JsonSerializable(typeof(int))]
public partial class KeyActionConfigJsonSgContext : JsonSerializerContext
{
}
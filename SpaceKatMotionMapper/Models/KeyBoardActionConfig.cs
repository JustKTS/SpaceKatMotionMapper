using System.Text.Json.Serialization;
using WindowsInput;

namespace SpaceKatMotionMapper.Models;

public record KeyBoardActionConfig(VirtualKeyCode Key, PressModeEnum PressMode)
{
    public KeyBoardActionConfig() : this(VirtualKeyCode.None, PressModeEnum.None)
    {
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KeyBoardActionConfig))]
[JsonSerializable(typeof(VirtualKeyCode))]
[JsonSerializable(typeof(PressModeEnum))]
internal partial class KeyBoardActionConfigJsonSgContext : JsonSerializerContext
{
}
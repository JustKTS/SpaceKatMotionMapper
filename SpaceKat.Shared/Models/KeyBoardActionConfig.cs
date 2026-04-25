using System.Text.Json.Serialization;

namespace SpaceKat.Shared.Models;

public record KeyBoardActionConfig(KeyCodeWrapper Key, PressModeEnum PressMode)
{
    public KeyBoardActionConfig() : this(KeyCodeWrapper.NONE, PressModeEnum.None)
    {
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KeyBoardActionConfig))]
[JsonSerializable(typeof(KeyCodeWrapper))]
[JsonSerializable(typeof(PressModeEnum))]
public partial class KeyBoardActionConfigJsonSgContext : JsonSerializerContext
{
}
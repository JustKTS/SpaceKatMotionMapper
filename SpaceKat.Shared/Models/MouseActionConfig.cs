using System.Text.Json.Serialization;

namespace SpaceKat.Shared.Models;

public record MouseActionConfig(MouseButtonEnum Key, PressModeEnum PressMode, int Multiplier)
{
    public MouseActionConfig() : this(MouseButtonEnum.None, PressModeEnum.None, 1)
    {
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MouseActionConfig))]
[JsonSerializable(typeof(MouseButtonEnum))]
[JsonSerializable(typeof(PressModeEnum))]
[JsonSerializable(typeof(int))]
public partial class MouseActionConfigJsonSgContext : JsonSerializerContext
{
}
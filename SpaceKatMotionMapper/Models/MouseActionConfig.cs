using System.Text.Json.Serialization;

namespace SpaceKatMotionMapper.Models;

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
internal partial class MouseActionConfigJsonSgContext : JsonSerializerContext
{
}
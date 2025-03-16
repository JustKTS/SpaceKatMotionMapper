using System.Text.Json.Serialization;

namespace SpaceKatMotionMapper.Models;

public record DelayActionConfig(int Milliseconds);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DelayActionConfig))]
[JsonSerializable(typeof(int))]
internal partial class DelayActionConfigJsonSgContext : JsonSerializerContext
{
}
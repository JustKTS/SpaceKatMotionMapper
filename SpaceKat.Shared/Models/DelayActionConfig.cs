using System.Text.Json.Serialization;

namespace SpaceKat.Shared.Models;

public record DelayActionConfig(int Milliseconds);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DelayActionConfig))]
[JsonSerializable(typeof(int))]
public partial class DelayActionConfigJsonSgContext : JsonSerializerContext
{
}
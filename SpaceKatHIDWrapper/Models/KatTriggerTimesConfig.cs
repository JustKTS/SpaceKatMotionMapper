using System.Text.Json.Serialization;

namespace SpaceKatHIDWrapper.Models;

public record KatTriggerTimesConfig(
    int ShortRepeatToleranceMs,
    int LongReachTimeoutMs,
    int LongReachRepeatTimeSpanMs,
    double LongReachRepeatScaleFactor)
{
    public KatTriggerTimesConfig() : this(200, 800, 100, 1.5)
    {
    }
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatTriggerTimesConfig))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(double))]
public partial class KatTriggerTimesConfigJsonSgContext : JsonSerializerContext
{
}
using System.Text.Json.Serialization;

namespace SpaceKatHIDWrapper.Models;

public record KatDeadZoneConfig(double[] Upper, double[] Lower)
{
    public KatDeadZoneConfig() : this([0.4,0.4,0.4,0.3,0.3,0.3], [-0.4,-0.4,-0.4,-0.3,-0.3,-0.3])
    {
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatDeadZoneConfig))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(double[]))]
public partial class KatDeadZoneConfigJsonSgContext : JsonSerializerContext
{
}
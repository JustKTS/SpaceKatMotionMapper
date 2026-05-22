using System.Text.Json.Serialization;

namespace SpaceKatHIDWrapper.Models;

// public record KatDeadZoneConfig(double[] Upper, double[] Lower)
// {
//     public KatDeadZoneConfig() : this([0.4,0.4,0.4,0.3,0.3,0.3], [-0.4,-0.4,-0.4,-0.3,-0.3,-0.3])
//     {
//     }
// }

public record KatDeadZoneConfig(double[] Upper, double[] Lower, bool[] AxesInverse)
{
    public KatDeadZoneConfig() : this([0.4,0.4,0.4,0.3,0.3,0.3], [-0.4,-0.4,-0.4,-0.3,-0.3,-0.3], [false, false, false, false, false, false])
    {
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatDeadZoneConfig))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(double[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(bool[]))]
public partial class KatDeadZoneConfigJsonSgContext : JsonSerializerContext
{
}
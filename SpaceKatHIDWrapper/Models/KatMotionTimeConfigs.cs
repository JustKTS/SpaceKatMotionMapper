using System.Text.Json.Serialization;

namespace SpaceKatHIDWrapper.Models;

public record KatMotionTimeConfigs(Dictionary<KatMotionEnum, KatTriggerTimesConfig> Configs)
{
    public KatMotionTimeConfigs() : this(new Dictionary<KatMotionEnum, KatTriggerTimesConfig>())
    {
        Configs = MotionTimeConfigInitHelper.GeneDefault();
    }
}

public static class MotionTimeConfigInitHelper
{
    public static Dictionary<KatMotionEnum, KatTriggerTimesConfig> GeneDefault()
    {
        return KatMotionEnumExtensions.GetValues().Where(e => e is not (KatMotionEnum.Null or KatMotionEnum.Stable))
            .ToDictionary(motion => motion, _ => new KatTriggerTimesConfig());
    }
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatMotionTimeConfigs))]
[JsonSerializable(typeof(KatMotionEnum))]
[JsonSerializable(typeof(Dictionary<KatMotionEnum, KatTriggerTimesConfig>))]
[JsonSerializable(typeof(KatTriggerTimesConfig))]
public partial class KatMotionTimeConfigsJsonSgContext : JsonSerializerContext
{
}
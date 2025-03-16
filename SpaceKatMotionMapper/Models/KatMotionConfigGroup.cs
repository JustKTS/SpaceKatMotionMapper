using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatMotionConfigGroup(
    string Guid,
    bool IsDefault,
    string ProcessPath,
    List<KatMotionConfig> Motions,
    bool IsCustomDeadZone,
    KatDeadZoneConfig DeadZoneConfig,
    bool IsCustomMotionTimeConfigs,
    KatMotionTimeConfigs MotionTimeConfigs)
{
    public KatMotionConfigGroup(
        bool IsDefault,
        string ProcessPath,
        List<KatMotionConfig> configs) : this(System.Guid.NewGuid().ToString(), IsDefault, ProcessPath,
        configs, false, new KatDeadZoneConfig(), false, new KatMotionTimeConfigs())
    {
    }

    public KatMotionConfigGroup() : this(
        System.Guid.NewGuid().ToString(),
        false,
        "",
        [], false, new KatDeadZoneConfig(), false, new KatMotionTimeConfigs())
    {
    }
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatMotionConfigGroup))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(List<KatMotionConfig>))]
[JsonSerializable(typeof(KatDeadZoneConfig))]
[JsonSerializable(typeof(KatMotionTimeConfigs))]
internal partial class KatMotionConfigGroupJsonSgContext : JsonSerializerContext
{
}
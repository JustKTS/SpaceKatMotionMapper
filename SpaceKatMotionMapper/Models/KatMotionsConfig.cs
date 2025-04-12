using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatMotionConfig(
    KatMotion Motion,
    List<KeyActionConfig> ActionConfigs,
    bool IsCustomDescription = false,
    string KeyActionsDescription = "",
    int ModeNum = 0,
    int ToModeNum = 0)
{
    public KatMotionConfig() : this(
        new KatMotion(KatMotionEnum.Null,
            KatPressModeEnum.Short,
            0),
        [])
    {
    }
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatMotionConfig))]
[JsonSerializable(typeof(List<KeyActionConfig>))]
[JsonSerializable(typeof(int))]
internal partial class KatMotionConfigJsonSgContext : JsonSerializerContext
{
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public enum KatConfigModeEnum
{
    [Display(Name = "单动作模式")]
    SingleAction = 0,
    [Display(Name = "进阶模式")]
    Advanced = 1,
    [Display(Name = "专家模式")]
    Expert = 2
}

public record KatMotionConfig(
    KatMotion Motion,
    List<KeyActionConfig> ActionConfigs,
    bool IsCustomDescription = false,
    string KeyActionsDescription = "",
    int ModeNum = 0,
    int ToModeNum = 0,
    KatConfigModeEnum ConfigMode = KatConfigModeEnum.Advanced)
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

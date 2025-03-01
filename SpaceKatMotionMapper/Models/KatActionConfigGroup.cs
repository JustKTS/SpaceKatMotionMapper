using System.Collections.Generic;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatActionConfigGroup(
    string Guid,
    bool IsDefault,
    string Name,
    string ProcessPath,
    List<KatActionConfig> Actions,
    bool IsCustomDeadZone,
    KatDeadZoneConfig DeadZoneConfig,
    bool IsCustomMotionTimeConfigs,
    KatMotionTimeConfigs MotionTimeConfigs)
{
    public KatActionConfigGroup(
        bool IsDefault,
        string Name,
        string ProcessPath,
        List<KatActionConfig> Configs) : this(System.Guid.NewGuid().ToString(), IsDefault, Name, ProcessPath,
        Configs, false, new KatDeadZoneConfig(), false, new KatMotionTimeConfigs(IsDefault))
    {
    }

    public KatActionConfigGroup() : this(
        System.Guid.NewGuid().ToString(),
        false,
        "",
        "",
        [], false, new KatDeadZoneConfig(), false, new KatMotionTimeConfigs(false))
    {
    }
}

public static class KatActionConfigGroupHelper
{
    public static KatActionConfigGroup CreateFromCustomConfigs(KatActionConfigGroup configGroup)
    {
        return configGroup with
        {
            Guid = System.Guid.NewGuid().ToString(), IsDefault = false, Name = "", ProcessPath = "", Actions = []
        };
    }
}
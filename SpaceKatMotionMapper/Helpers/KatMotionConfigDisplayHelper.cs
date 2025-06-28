using System.Collections.Generic;
using System.Linq;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Models;
 

namespace SpaceKatMotionMapper.Helpers;

public static class KatMotionConfigDisplayHelper
{
    public static KeyActionConfig[] GetKeyActionsDescriptions(this KatMotionConfig config)
    {
        return config.IsCustomDescription
            ? [new KeyActionConfig(ActionType.None, config.KeyActionsDescription, PressModeEnum.None, 0)]
            : GenerateDisplayList(config.ActionConfigs);
    }
    public static KeyActionConfig[] GenerateDisplayList(List<KeyActionConfig> actionConfigs)
    {
        //TODO: 优化显示内容，增加特殊快捷键
        if (actionConfigs.Count == 1)
        {
            return [actionConfigs.First()];
        }

        return CombinationKeysHelper.ValidateIsCombinationKeys(actionConfigs)
            ? CombinationKeysHelper.GenerateCombinationKeys(actionConfigs)
            : actionConfigs.ToArray();
    }

}
using System.Collections.Generic;
using System.Linq;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Models;

// ReSharper disable StringLiteralTypo

namespace SpaceKatMotionMapper.Helpers;

public static class KatMotionConfigDisplayHelper
{
    public static KeyActionConfig[] GetKeyActionsDescriptions(this KatMotionConfig config)
    {
        return config.IsCustomDescription
            ? [new KeyActionConfig(ActionType.None, config.KeyActionsDescription, PressModeEnum.None, 0)]
            : GenerateDisplayList(config.ActionConfigs);
    }

    private static readonly string[] ModifierKeys = ["CONTROL", "ALT", "SHIFT", "WIN"];

    private static string RemoveLorR(string key) => key switch
    {
        "LCONTROL" or "RCONTROL" => "CONTROL",
        "LALT" or "RALT" => "ALT",
        "LSHIFT" or "RSHIFT" => "SHIFT",
        "LWIN" or "RWIN" => "WIN",
        _ => key
    };

    public static KeyActionConfig[] GenerateDisplayList(List<KeyActionConfig> actionConfigs)
    {
        //TODO: 优化显示内容，增加特殊快捷键
        if (actionConfigs.Count == 1)
        {
            return [actionConfigs.First()];
        }

        return ValidateIsCombinationKeys(actionConfigs)
            ? GenerateCombinationKeys(actionConfigs)
            : actionConfigs.ToArray();
    }

    private static bool ValidateIsCombinationKeys(List<KeyActionConfig> actionConfigs)
    {
        return actionConfigs
            .Select(c => RemoveLorR(c.Key)) // 取出所有的Key并去除LR
            .Where(key => ModifierKeys.Contains(key)) // 过滤出组合键
            .GroupBy(key => key) // 分组
            .Any(group => group.Count() > 1); // 判断是否有重复的组合键
    }


    private static KeyActionConfig[] GenerateCombinationKeys(List<KeyActionConfig> actionConfigs)
    {
        var allKeys = actionConfigs
            .Select(c => c with { Key = RemoveLorR(c.Key) }) // 取出所有的Key并去除LR
            .GroupBy(c => c.Key).Select(group => group.First()).ToArray();
        var modifierKeys = allKeys
            .Where(c => ModifierKeys.Contains(c.Key));
        var keys = allKeys
            .Where(c => !ModifierKeys.Contains(c.Key));
        return modifierKeys.ToArray().Concat(keys.ToArray()).ToArray();
    }
}
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;

// ReSharper disable StringLiteralTypo

namespace SpaceKat.Shared.Functions;

public static class CombinationKeysHelper
{
    private static readonly string[] ModifierKeys = ["CONTROL", "ALT", "SHIFT", "WIN"];

    private static string RemoveLorR(string key) => key switch
    {
        "LCONTROL" or "RCONTROL" => "CONTROL",
        "LALT" or "RALT" => "ALT",
        "LSHIFT" or "RSHIFT" => "SHIFT",
        "LWIN" or "RWIN" => "WIN",
        _ => key
    };
    
    public static bool ValidateIsCombinationKeys(List<KeyActionConfig> actionConfigs)
    {
        return actionConfigs
            .Select(c => RemoveLorR(c.Key)) // 取出所有的Key并去除LR
            .Where(key => ModifierKeys.Contains(key)) // 过滤出组合键
            .GroupBy(key => key) // 分组
            .Any(group => group.Count() > 1); // 判断是否有重复的组合键
    }


    public static KeyActionConfig[] GenerateCombinationKeys(List<KeyActionConfig> actionConfigs)
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
    
    public static CombinationKeysRecord ConvertActionsToCombinationRecord(List<KeyActionConfig> actionConfigs)
    {
        var allKeys = actionConfigs
            .Select(c => c with { Key = RemoveLorR(c.Key) }) // 取出所有的Key并去除LR
            .GroupBy(c => c.Key).Select(group => group.First()).ToArray();
        var modifierKeys = allKeys
            .Where(c => ModifierKeys.Contains(c.Key));
        var keys = allKeys
            .Where(c => !ModifierKeys.Contains(c.Key));
        var useCtrl = false;
        var useShift = false;
        var useWin = false;
        var useAlt = false;
        modifierKeys.Iter(e =>
        {
            switch (e.Key)
            {
                case "CONTROL":
                    useCtrl = true;
                    break;
                case "ALT":
                    useAlt = true;
                    break;
                case "WIN":
                    useWin = true;
                    break;
                case "Shift":
                    useShift = true;
                    break;
            }
        });
        return new CombinationKeysRecord(useCtrl, useShift, useAlt, useWin, VirtualKeyHelpers.Parse(keys.First().Key));
    }
}
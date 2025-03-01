using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using WindowsInput;

namespace SpaceKatMotionMapper.Helpers;

public static class VirtualKeyHelper
{
    private static FrozenDictionary<string, VirtualKeyCode> KeyDict { get; }
    public static IReadOnlyList<string> KeyNames { get; }

    static VirtualKeyHelper()
    {
        var keyDict = new Dictionary<string, VirtualKeyCode>();
        foreach (VirtualKeyCode key in Enum.GetValues(typeof(VirtualKeyCode)))
        {
            // if (key is VirtualKeyCode.None) continue;
            _ = keyDict.TryAdd(Enum.GetName(key)!, key);
        }

        KeyDict = keyDict.ToFrozenDictionary();
        KeyNames = keyDict.Keys
            // ReSharper disable once StringLiteralTypo
            .Where(x => x is not ("None" or "LBUTTON" or "RBUTTON" or "MBUTTON" or "XBUTTON1" or "XBUTTON2"))
            .ToList()
            .AsReadOnly();
    }

    public static VirtualKeyCode Parse(string key)
    {
        return KeyDict.GetValueOrDefault(key, VirtualKeyCode.None);
    }
}

public class VirtualKeyCodeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not VirtualKeyCode keyCode ? string.Empty : Enum.GetName(keyCode);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? VirtualKeyCode.None : VirtualKeyHelper.Parse(keyName);
    }
}
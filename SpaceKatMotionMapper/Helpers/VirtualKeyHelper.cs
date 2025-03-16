using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SpaceKatMotionMapper.Models;
using WindowsInput;

namespace SpaceKatMotionMapper.Helpers;

public static class VirtualKeyHelper
{
    private static FrozenDictionary<string, VirtualKeyCode> KeyDict { get; }
    public static IReadOnlyList<string> KeyNames { get; }
    
    static VirtualKeyHelper()
    {
        var keyDict = new Dictionary<string, VirtualKeyCode>();
        foreach (var key in KeyCodeWrapperExtensions.GetValues())
        {
            _ = keyDict.TryAdd(key.ToStringFast(), (VirtualKeyCode)key);
        }
        
        KeyDict = keyDict.ToFrozenDictionary();
        KeyNames = KeyCodeWrapperExtensions.GetNames()
            .ToList()
            .AsReadOnly();
    }

    public static VirtualKeyCode Parse(string key)
    {
        return KeyDict.GetValueOrDefault(key, VirtualKeyCode.None);
    }

    public static string WrapKeyCodeName(VirtualKeyCode keyCode)
    {
        return ((KeyCodeWrapper)keyCode).ToStringFast();
    }
}

public class VirtualKeyCodeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not VirtualKeyCode keyCode ? string.Empty : ((KeyCodeWrapper)keyCode).ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? VirtualKeyCode.None : VirtualKeyHelper.Parse(keyName);
    }
}

public static class VirtualKeyCodeExtension
{
    public static string ToWarpCodeName(this VirtualKeyCode keyCode)
    {
        return VirtualKeyHelper.WrapKeyCodeName(keyCode);
    }
}
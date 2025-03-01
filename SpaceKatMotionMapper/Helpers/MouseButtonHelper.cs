using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SpaceKatMotionMapper.Models;
using WindowsInput;

namespace SpaceKatMotionMapper.Helpers;

public static class MouseButtonHelper
{
    public static IReadOnlyList<string> ButtonNames { get; } =
        MouseButtonEnumExtensions.GetValues()
            .Where(x => x is not MouseButtonEnum.None)
            .Select(x => x.ToStringFast())
            .ToList().AsReadOnly();
    
    public static MouseButtonEnum Parse(string key)
    {
        var ret = MouseButtonEnumExtensions.TryParse(key, out var buttonEnum, ignoreCase: true, allowMatchingMetadataAttribute:true);
        return ret ? buttonEnum : MouseButtonEnum.None;
    }
}

public class MouseButtonEnumConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not MouseButtonEnum keyCode ? string.Empty : keyCode.ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? MouseButtonEnum.None : MouseButtonHelper.Parse(keyName);
    }
}

public class ShowBaseOnMouseScrollConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string keyCodeStr) return false;
        var keyCode = MouseButtonHelper.Parse(keyCodeStr);
        return keyCode switch
        {
            MouseButtonEnum.ScrollUp or MouseButtonEnum.ScrollDown => true,
            _ => false
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        null;
}

public class HideBaseOnMouseScrollConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string keyCodeStr) return true;
        var keyCode = MouseButtonHelper.Parse(keyCodeStr);
        return keyCode switch
        {
            MouseButtonEnum.ScrollUp or MouseButtonEnum.ScrollDown => false,
            _ => true
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        null;
}
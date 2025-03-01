using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SpaceKatHIDWrapper.Models;


namespace SpaceKatMotionMapper.Helpers;

public static class KatMotionHelper
{
    public static IReadOnlyList<string> KatMotionNames { get; } =
        KatMotionEnumExtensions.GetValues()
            .Where(x => x is not (KatMotionEnum.Null or KatMotionEnum.Stable))
            .Select(x => x.ToStringFast())
            .ToList().AsReadOnly();

    public static IReadOnlyList<string> KatPressModeNames { get; } =
        KatPressModeEnumExtensions.GetValues()
            .Where(x => x is not KatPressModeEnum.Null)
            .Select(x => x.ToStringFast()).ToList().AsReadOnly();


    public static KatMotionEnum ParseKatMotionEnum(string key)
    {
        var ret = KatMotionEnumExtensions.TryParse(key, out var katMotion, ignoreCase: true,
            allowMatchingMetadataAttribute: true);
        return ret ? katMotion : KatMotionEnum.Null;
    }

    public static KatPressModeEnum ParsePressModeEnum(string key)
    {
        var ret = KatPressModeEnumExtensions.TryParse(key, out var pressMode, ignoreCase: true,
            allowMatchingMetadataAttribute: true);
        return ret ? pressMode : KatPressModeEnum.Null;
    }
}

public class KatMotionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not KatMotionEnum key ? string.Empty : key.ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? KatMotionEnum.Null : KatMotionHelper.ParseKatMotionEnum(keyName);
    }
}

public class KatPressModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not KatPressModeEnum key ? string.Empty : key.ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? KatPressModeEnum.Null : KatMotionHelper.ParsePressModeEnum(keyName);
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Helpers;

public static class PressModeHelper
{
    public static IReadOnlyList<string> PressModeNames { get; } =
        PressModeEnumExtensions.GetValues()
            .Where(x => x is not PressModeEnum.None)
            .Select(x => x.ToStringFast()).ToList().AsReadOnly();


    public static PressModeEnum Parse(string pressModeName)
    {
        var ret = PressModeEnumExtensions.TryParse(pressModeName, out var pressModeEnum, ignoreCase: true,
            allowMatchingMetadataAttribute: true);
        return ret ? pressModeEnum : PressModeEnum.None;
    }
}

public class PressModeEnumConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not PressModeEnum keyMode ? PressModeEnum.None.ToStringFast() : keyMode.ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string pressModeStr ? PressModeEnum.None : PressModeHelper.Parse(pressModeStr);
    }
}

public class DoubleClickCheckConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ActionType actionType) return new List<string>();
        return actionType is ActionType.Mouse
            ? PressModeHelper.PressModeNames
            : PressModeHelper.PressModeNames.Where(x => x != "双击").ToList().AsReadOnly();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        null;
}
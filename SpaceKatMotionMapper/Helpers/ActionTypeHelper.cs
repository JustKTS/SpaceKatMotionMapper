using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SpaceKatMotionMapper.Models;
using WindowsInput;

namespace SpaceKatMotionMapper.Helpers;

public static class ActionTypeHelper
{
    public static IReadOnlyList<string> ActionTypeNames { get; } = ActionTypeExtensions.GetValues()
        .Select(type => type.ToStringFast()).ToList().AsReadOnly();

    public static ActionType Parse(string actionTypeStr)
    {
        var ret = ActionTypeExtensions.TryParse(actionTypeStr, out var actionType, ignoreCase: true,
            allowMatchingMetadataAttribute: true);
        return ret ? actionType : ActionType.KeyBoard;
    }
}

public class ActionTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not ActionType actionType ? string.Empty : actionType.ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string actionTypeStr ? ActionType.KeyBoard : ActionTypeHelper.Parse(actionTypeStr);
    }
}

public class ActionTypeItemNamesConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string actionTypeStr) return VirtualKeyHelper.KeyNames;
        var actionType = ActionTypeHelper.Parse(actionTypeStr);

        return actionType switch
        {
            ActionType.KeyBoard => VirtualKeyHelper.KeyNames,
            ActionType.Mouse => MouseButtonHelper.ButtonNames,
            _ => VirtualKeyHelper.KeyNames
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => null;
}

public class ActionTypeKeyOrButtonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string actionTypeStr) return string.Empty;
        return actionTypeStr == "None" ? string.Empty : actionTypeStr;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string keyStr) return string.Empty;
        if (string.IsNullOrEmpty(keyStr) || string.IsNullOrWhiteSpace(keyStr)) return "None";

        try
        {
            var ret = MouseButtonEnumExtensions.TryParse(keyStr, out var button, ignoreCase: true,
                allowMatchingMetadataAttribute: true);
            if (ret)
            {
                return button.ToStringFast();
            }

            var keyCode = VirtualKeyHelper.Parse(keyStr);
            return keyCode.ToString();
        }
        catch (Exception e)
        {
            return "None";
        }
       
    }
}
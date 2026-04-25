using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Serilog;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;


namespace SpaceKatMotionMapper.Helpers;

public static class KatMotionHelper
{
    public static IReadOnlyList<string> KatMotionNames { get; } =
        KatMotionEnumExtensions.GetValues()
            .Where(x => x is not (KatMotionEnum.Null or KatMotionEnum.Stable))
            .Select(x => x.ToStringFast(useMetadataAttributes:true))
            .ToList().AsReadOnly();

    public static IReadOnlyList<string> KatPressModeNames { get; } =
        KatPressModeEnumExtensions.GetValues()
            .Where(x => x is not KatPressModeEnum.Null)
            .Select(x => x.ToStringFast(useMetadataAttributes:true)).ToList().AsReadOnly();

    public static IReadOnlyList<string> KatPressModeSimpleNames { get; } =
        new List<string> { "短触", "长推" }.AsReadOnly();

    public static IReadOnlyList<string> KatConfigModeNames { get; } =
        new List<string> { "单动作", "进阶", "专家" }.AsReadOnly();


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

    public static KatConfigModeEnum ParseConfigModeEnum(string key)
    {
        return key switch
        {
            "单动作" => KatConfigModeEnum.SingleAction,
            "进阶" => KatConfigModeEnum.Advanced,
            "专家" => KatConfigModeEnum.Expert,
            _ => KatConfigModeEnum.Advanced
        };
    }
}

public class KatMotionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not KatMotionEnum key ? string.Empty : key.ToStringFast(useMetadataAttributes:true);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string keyName)
        {
            var result = KatMotionHelper.ParseKatMotionEnum(keyName);
            Log.Information("[UI绑定] KatMotionConverter ConvertBack: '{KeyName}' -> {Result}", keyName, result);
            return result;
        }
        Log.Information("[UI绑定] KatMotionConverter ConvertBack: 无效值 {Value}, 返回 Null", value);
        return KatMotionEnum.Null;
    }
}

public class KatPressModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not KatPressModeEnum key ? string.Empty : key.ToStringFast(useMetadataAttributes:true);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string keyName)
        {
            var result = KatMotionHelper.ParsePressModeEnum(keyName);
            Log.Information("[UI绑定] KatPressModeConverter ConvertBack: '{KeyName}' -> {Result}", keyName, result);
            return result;
        }
        Log.Information("[UI绑定] KatPressModeConverter ConvertBack: 无效值 {Value}, 返回 Null", value);
        return KatPressModeEnum.Null;
    }
}

public class KatPressModeSimpleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            KatPressModeEnum.Short => "短触",
            KatPressModeEnum.LongReach => "长推",
            KatPressModeEnum.LongDown => "长推",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? KatPressModeEnum.Null :
               keyName == "短触" ? KatPressModeEnum.Short :
               KatPressModeEnum.LongReach;
    }
}

public class KatConfigModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not KatConfigModeEnum key ? string.Empty : key switch
        {
            KatConfigModeEnum.SingleAction => "单动作",
            KatConfigModeEnum.Advanced => "进阶",
            KatConfigModeEnum.Expert => "专家",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? KatConfigModeEnum.Advanced : KatMotionHelper.ParseConfigModeEnum(keyName);
    }
}
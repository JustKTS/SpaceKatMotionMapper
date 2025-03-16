using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Helpers;

namespace SpaceKatMotionMapper.Views;

public partial class KatMotionConfigView : UserControl
{
    public KatMotionConfigView()
    {
        InitializeComponent();
    }
}

public class ModeNumToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int num) return false;
        return num > 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        null;
}

public class ShortTriggerToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string katPressModeStr) return false;
        var pressMode = KatMotionHelper.ParsePressModeEnum(katPressModeStr);
        return pressMode is KatPressModeEnum.Short;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        null;
}
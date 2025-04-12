using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.CustomControls;

public partial class KeyActionsDescriptionDisplay : UserControl
{
    public static readonly StyledProperty<KeyActionConfig[]> DisplayItemsProperty =
        AvaloniaProperty.Register<DeadZoneIndicator, KeyActionConfig[]>(nameof(DisplayItems),
            defaultBindingMode: BindingMode.OneWay,
            defaultValue: []);

    public KeyActionConfig[] DisplayItems
    {
        get => GetValue(DisplayItemsProperty);
        set =>
            SetValue(DisplayItemsProperty, value);
    }

    public KeyActionsDescriptionDisplay()
    {
        InitializeComponent();
        DisplayItemsProperty.Changed
            .AddClassHandler<KeyActionsDescriptionDisplay, KeyActionConfig[]>(
                (o, e) => o.OnDisplayItemsChanged(e)
            );
    }

    private void OnDisplayItemsChanged(AvaloniaPropertyChangedEventArgs<KeyActionConfig[]> args)
    {
        DisplayItemsControl.ItemsSource = args.NewValue.Value;
    }
}

public sealed class KeyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return string.Empty;
        return str == "None" ? string.Empty : str;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class ActionTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ActionType actionType ? ActionTypeHelper.ToPathIconGeometry(actionType) : new StreamGeometry();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class ActionTypeNoneToFalseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ActionType actionType) return false;
        return actionType != ActionType.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class ActionTypeNoneToTrueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ActionType actionType) return false;
        return actionType == ActionType.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class DisplayMultiplierConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not KeyActionConfig action) return false;
        if (action.ActionType != ActionType.Mouse) return false;
        
        var ret = action.TryToMouseActionConfig(out var mouseActionConfig);
        if (!ret) return false;
        return mouseActionConfig!.Key is MouseButtonEnum.ScrollDown or MouseButtonEnum.ScrollUp
            or MouseButtonEnum.ScrollLeft or MouseButtonEnum.ScrollRight;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
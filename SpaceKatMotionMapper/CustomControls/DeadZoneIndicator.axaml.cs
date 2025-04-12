using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Ursa.Controls;

namespace SpaceKatMotionMapper.CustomControls;

public partial class DeadZoneIndicator : UserControl
{
    # region 外层数值

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<DeadZoneIndicator, double>(nameof(Maximum), defaultValue: 1.5);

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<DeadZoneIndicator, double>(nameof(Minimum), defaultValue: -1.5);

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    # endregion

    public static readonly StyledProperty<double> DeadZoneUpperProperty =
        AvaloniaProperty.Register<DeadZoneIndicator, double>(nameof(DeadZoneUpper),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: 0.2);

    public double DeadZoneUpper
    {
        get => GetValue(DeadZoneUpperProperty);
        set => SetValue(DeadZoneUpperProperty, value);
    }

    public static readonly StyledProperty<double> DeadZoneLowerProperty =
        AvaloniaProperty.Register<DeadZoneIndicator, double>(nameof(DeadZoneLower),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: 0.2);

    public double DeadZoneLower
    {
        get => GetValue(DeadZoneLowerProperty);
        set => SetValue(DeadZoneLowerProperty, value);
    }

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<DeadZoneIndicator, double>(nameof(Value), defaultValue: 0,
            defaultBindingMode: BindingMode.OneWay);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    static DeadZoneIndicator()
    {
        DeadZoneUpperProperty.Changed
            .AddClassHandler<DeadZoneIndicator, double>(
                (o, e) => o.OnDeadZoneUpperChange(e)
            );
        DeadZoneLowerProperty.Changed
            .AddClassHandler<DeadZoneIndicator, double>(
                (o, e) => o.OnDeadZoneLowerChange(e)
            );

        ValueProperty.Changed
            .AddClassHandler<DeadZoneIndicator, double>(
                (o, e) => o.OnValueChange(e)
            );
    }

    private void OnDeadZoneUpperChange(AvaloniaPropertyChangedEventArgs<double> args)
    {
        DeadZoneRangeSlider.UpperValue = args.NewValue.Value;
    }

    private void OnDeadZoneLowerChange(AvaloniaPropertyChangedEventArgs<double> args)
    {
        DeadZoneRangeSlider.LowerValue = args.NewValue.Value;
    }

    private void OnValueChange(AvaloniaPropertyChangedEventArgs<double> args)
    {
        var width = Bounds.Width;
        var left = (args.NewValue.Value - Minimum) / (Maximum - Minimum) * width - IndicatorBorder.Width / 2;
        if (IndicatorBorder.RenderTransform is not TranslateTransform transform) return;
        transform.X = left;
        // IndicatorBorder.Margin = new Thickness(left, 8, 0, 8);
    }

    public DeadZoneIndicator()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        DeadZoneRangeSlider.Maximum = Maximum;
        DeadZoneRangeSlider.Minimum = Minimum;
        DeadZoneRangeSlider.UpperValue = DeadZoneUpper;
        DeadZoneRangeSlider.LowerValue = DeadZoneLower;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var left = 0.5 * Bounds.Width - IndicatorBorder.Width / 2;
        if (IndicatorBorder.RenderTransform is not TranslateTransform transform) return;
        transform.X = left;
        // IndicatorBorder.Margin = new Thickness(left, 8, 0, 8);
    }

    private void DeadZoneRangeSlider_OnValueChanged(object? sender, RangeValueChangedEventArgs e)
    {
        if (e.IsLower)
        {
            DeadZoneLower = e.NewValue;
        }
        else
        {
            DeadZoneUpper = e.NewValue;
        }
    }
}
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class TransparentInfoWindow : UrsaWindow
{
    public TransparentInfoWindow()
    {
        DataContext = App.GetRequiredService<TransparentInfoViewModel>();
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        App.GetRequiredService<TransparentInfoService>().LoadConfigs().ContinueWith(task =>
        {
            var config = task.Result;
            if (config is null)
            {
                var screen = Screens.Primary;
                if (screen == null) return;

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var area = screen.WorkingArea;
                    var windowHeight = area.Height / 20;
                    Height = windowHeight;
                    Width = windowHeight * 5;
                    Position = new PixelPoint(area.X + area.Width - (int)Width - 10,
                        area.Y + area.Height - (int)Height - 10);
                    (DataContext as TransparentInfoViewModel)!.BackgroundColor = new Color(0x66, 0xD3, 0xD3, 0xD3);
                    (DataContext as TransparentInfoViewModel)!.FontSize = 15;
                    (DataContext as TransparentInfoViewModel)!.DisappearTimeMs = 1500;
                    (DataContext as TransparentInfoViewModel)!.AnimationTimeMs = 250;
                });
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Height = config.Height;
                    Width = config.Width;
                    Position = new PixelPoint(config.X, config.Y);

                    (DataContext as TransparentInfoViewModel)!.BackgroundColor =
                        Color.FromUInt32(config.BackgroundColor);
                    if (config.FontSize ==0 ) 
                        (DataContext as TransparentInfoViewModel)!.FontSize = 15;
                    else
                    {
                        (DataContext as TransparentInfoViewModel)!.FontSize = config.FontSize;
                    }
                    (DataContext as TransparentInfoViewModel)!.DisappearTimeMs = config.DisappearTimeMs;
                    (DataContext as TransparentInfoViewModel)!.AnimationTimeMs = config.AnimationTimeMs;
                });
            }
        });
    }

    public void SetOpacity(int value)
    {
        InfoPanel.Opacity = value;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse) BeginMoveDrag(e);
    }

    private void SaveConfigButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var vm = (DataContext as TransparentInfoViewModel)!;
        vm.SaveConfig(Position.X, Position.Y, Width, Height);
    }
}

public class TimeSpanMsConverter:IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return TimeSpan.FromMilliseconds(value is not int timeSpanMs ? 250 : timeSpanMs);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
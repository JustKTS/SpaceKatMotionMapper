using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class TransparentInfoWindow : UrsaWindow
{
    public TransparentInfoWindow()
    {
        DataContext = App.GetRequiredService<TransparentInfoViewModel>();
        ShowActivated = false;

        App.GetRequiredService<ILocalSettingsService>()
            .ReadSettingAsync<TransparentInfoWindowConfig>("TransparentInfoWindowConfig").ContinueWith((task) =>
            {
                var config = task.Result;
                if (config is null)
                {
                    var screen = Screens.Primary;
                    if (screen == null)
                    {
                        return;
                    }

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var area = screen.WorkingArea;
                        var windowHeight = area.Height / 20;
                        Height = windowHeight;
                        Width = windowHeight * 5;
                        Position = new PixelPoint(area.X + area.Width - (int)Width - 10,
                            area.Y + area.Height - (int)Height - 10);
                    });
                }
                else
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Height = config.Height;
                        Width = config.Width;
                        Position = new PixelPoint(config.X, config.Y);
                    });
                }
            });
        InitializeComponent();
    }

    public void SetOpacity(int value)
    {
        InfoPanel.Opacity = value;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse) this.BeginMoveDrag(e);
    }
}
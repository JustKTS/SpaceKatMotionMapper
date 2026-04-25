using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.NavVMs;

namespace SpaceKatMotionMapper.Views;

public partial class TitleBarRightContent : UserControl
{
    public TitleBarRightContent()
    {
        InitializeComponent();
    }
    // private readonly OverlayDialogOptions _options = new()
    // {
    //     FullScreen = true,
    //     HorizontalAnchor = HorizontalPosition.Center,
    //     VerticalAnchor = VerticalPosition.Center,
    //     HorizontalOffset = 0.0,
    //     VerticalOffset = 0.0,
    //     Mode = DialogMode.None,
    //     Buttons = DialogButton.None,
    //     Title = "设置",
    //     CanLightDismiss = true,
    //     CanDragMove = true,
    //     IsCloseButtonVisible = true,
    //     CanResize = false
    // };
    //
    // private void Button_OnClick(object? sender, RoutedEventArgs e)
    // {
    //     OverlayDialog.ShowModal<SettingsView, SettingsViewModel>(
    //         App.GetRequiredService<SettingsViewModel>(), MainWindow.LocalHost, options: _options);
    // }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var navVm = App.GetRequiredService<NavViewModel>();
        if (e.Source is not ToggleButton tb) return;
        switch (tb.IsChecked)
        {
            case null:
                return;
            case true:
                navVm.OnNavigation(navVm, typeof(SettingsView).FullName!);
                break;
            case false:
                navVm.OnNavigation(navVm, typeof(MainView).FullName!);
                break;
        }
    }
}
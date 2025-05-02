using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;
using WindowsInput;

namespace SpaceKatMotionMapper.Views;

public partial class KeyActionConfigView : UserControl
{
    public KeyActionConfigView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var useCtrl = UseCtrlCBox.IsChecked ?? false;
        var useShift = UseShiftCBox.IsChecked ?? false;
        var useWin = UseWinCBox.IsChecked ?? false;
        var useAlt = UseAltCBox.IsChecked ?? false;
        if (HotKeyTextBox.Text is not { } hotKeyStr) return;
        var hotKey = VirtualKeyHelpers.Parse(hotKeyStr);
        if (hotKey == VirtualKeyCode.None) return;
        (DataContext as KeyActionConfigViewModel)?.AddHotKeyActions(useCtrl, useWin, useAlt, useShift, hotKey);
        ((((e.Source as IconButton)?.Parent as Grid)?.Parent as FlyoutPresenter)
            ?.Parent as Popup)?.Close();
    }

    private void HotKeyTextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            textBox.Text = e.Key.ToVirtualKeyCode().GetWrappedName();
        });
    }
}
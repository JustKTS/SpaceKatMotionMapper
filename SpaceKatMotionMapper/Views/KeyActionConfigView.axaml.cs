using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;
using WindowsInput;

namespace SpaceKatMotionMapper.Views;

public partial class KeyActionConfigView : UrsaView
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
        if (HotKeyComboBox.SelectedItem is not string hotKeyStr) return;
        var hotKey = VirtualKeyHelper.Parse(hotKeyStr);
        if (hotKey == VirtualKeyCode.None) return;
        (DataContext as KeyActionConfigViewModel)?.AddHotKeyActions(useCtrl, useWin, useAlt, useShift, hotKey);
        ((((e.Source as IconButton)?.Parent as Grid)?.Parent as FlyoutPresenter)
            ?.Parent as Popup)?.Close();
    }
}
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

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
        var hotKey = HotKeyComboBox.SelectedItem is HotKeyCodeEnum hotKeyEnum
            ? hotKeyEnum
            : 0;
        if (hotKey == 0) return;
        (DataContext as KeyActionConfigViewModel)?.AddHotKeyActions(useCtrl, useWin, useAlt, useShift, hotKey);
        ((((e.Source as IconButton)?.Parent as Grid)?.Parent as FlyoutPresenter)
            ?.Parent as Popup)?.Close();
    }
}
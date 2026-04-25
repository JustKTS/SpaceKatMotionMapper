using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.ViewModels;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class KeyActionConfigView : UserControl
{
    public bool IsSingleActionMode
    {
        get => GetValue(IsSingleActionModeProperty);
        set => SetValue(IsSingleActionModeProperty, value);
    }

    public static readonly StyledProperty<bool> IsSingleActionModeProperty =
        AvaloniaProperty.Register<KeyActionConfigView, bool>(nameof(IsSingleActionMode));

    // private Avalonia.Controls.AutoCompleteBox? _keySelectionCBox;

    public KeyActionConfigView()
    {
        InitializeComponent();
        HotKeyTextBoxHelper.Register(HotKeyTextBox);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var useCtrl = UseCtrlCBox.IsChecked ?? false;
        var useShift = UseShiftCBox.IsChecked ?? false;
        var useWin = UseWinCBox.IsChecked ?? false;
        var useAlt = UseAltCBox.IsChecked ?? false;
        if (HotKeyTextBox.Text is not { } hotKeyStr) return;
        var hotKey = VirtualKeyHelpers.Parse(hotKeyStr);
        if (hotKey == KeyCodeWrapper.NONE) return;
        (DataContext as KeyActionConfigViewModel)?.AddHotKeyActions(useCtrl, useWin, useAlt, useShift, hotKey);
        ((((e.Source as IconButton)?.Parent as Grid)?.Parent as FlyoutPresenter)
            ?.Parent as Popup)?.Close();
    }

    private void FocusContainer_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not InputElement focusTarget) return;
        if (ShouldPreserveFocus(e.Source as StyledElement, focusTarget)) return;
        focusTarget.Focus();
    }

    private void KeySelectionCBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not Ursa.Controls.AutoCompleteBox { SelectedItem: string selectedKey }) return;
        if ((sender as Control)?.DataContext is not KeyActionWithCommandViewModel viewModel) return;
        viewModel.Key = selectedKey;
    }

    private static bool ShouldPreserveFocus(StyledElement? source, InputElement focusTarget)
    {
        for (var current = source; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, focusTarget)) return false;

            if (current is Ursa.Controls.AutoCompleteBox or TextBox or Button or ToggleButton or ComboBox or NumericIntUpDown or SelectionList)
            {
                return true;
            }

            if (current is InputElement { Focusable: true })
            {
                return true;
            }
        }

        return false;
    }
}
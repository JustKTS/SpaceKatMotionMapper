using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.NavVMs;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class TitleBarRightContent : UserControl
{
    public TitleBarRightContent()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachedToVisualTree -= OnAttachedToVisualTree;
        if (TopLevelHelper.GetTopLevel() is Window window)
        {
            window.PropertyChanged += OnWindowPropertyChanged;
            UpdateMaximizeIcon(window.WindowState);
        }
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty && sender is Window window)
        {
            UpdateMaximizeIcon(window.WindowState);
        }
    }

    private void UpdateMaximizeIcon(WindowState state)
    {
        if (MaximizeIcon is null) return;

        var iconKey = state == WindowState.Maximized
            ? "SemiIconRestore"
            : "SemiIconMaximize";

        if (Application.Current!.TryFindResource(iconKey, out var resource))
        {
            MaximizeIcon.Data = (Geometry)resource!;
        }
    }

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

    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevelHelper.GetTopLevel() is Window window)
        {
            window.Hide();
        }
    }

    private void MaximizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevelHelper.GetTopLevel() is Window window)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    private async void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevelHelper.GetTopLevel() is not Window window) return;

        var viewModel = new CloseConfirmDialogViewModel();
        var confirmed = await OverlayDialog.ShowCustomAsync<
            CloseConfirmDialog,
            CloseConfirmDialogViewModel,
            bool>(
            viewModel,
            MainWindow.LocalHost,
            CloseConfirmDialogViewModel.OverlayDialogOptions);

        if (confirmed)
        {
            window.Close();
        }
    }
}

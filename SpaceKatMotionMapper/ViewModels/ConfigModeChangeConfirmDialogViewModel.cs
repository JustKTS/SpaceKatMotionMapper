using System;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ConfigModeChangeConfirmDialogViewModel : ViewModelBase, IDialogContext
{
    public static readonly OverlayDialogOptions OverlayDialogOptions = new()
    {
        Buttons = DialogButton.None,
        HorizontalAnchor = HorizontalPosition.Center,
        VerticalAnchor = VerticalPosition.Center,
        CanDragMove = false,
        CanLightDismiss = false,
        CanResize = false,
        FullScreen = false,
        IsCloseButtonVisible = true,
        Mode = DialogMode.None,
        Title = "切换配置模式"
    };

    [RelayCommand]
    private void Confirm()
    {
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }

    public void Close()
    {
        RequestClose?.Invoke(this, false);
    }

    public event EventHandler<object?>? RequestClose;
}

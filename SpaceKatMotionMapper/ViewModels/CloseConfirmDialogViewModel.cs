using System;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class CloseConfirmDialogViewModel : ViewModelBase, IDialogContext
{
    public static readonly OverlayDialogOptions OverlayDialogOptions = new()
    {
        Buttons = DialogButton.None,
        HorizontalAnchor = HorizontalPosition.Right,
        VerticalAnchor = VerticalPosition.Top,
        HorizontalOffset = 12,
        VerticalOffset = 12,
        CanDragMove = false,
        CanLightDismiss = true,
        CanResize = false,
        FullScreen = false,
        IsCloseButtonVisible = true,
        Mode = DialogMode.None,
        Title = "退出确认"
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

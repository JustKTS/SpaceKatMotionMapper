using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class TitleBarRightContent : UserControl
{
    public TitleBarRightContent()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            App.GetRequiredService<PopUpNotificationService>().Pop(NotificationType.Error, "获取系统剪贴板权限失败");
            return;
        }

        _ = clipboard.SetTextAsync(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                nameof(SpaceKatMotionMapper))
        ).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Invoke(() =>
                App.GetRequiredService<PopUpNotificationService>().Pop(NotificationType.Success, "已复制到剪贴板"));
        });
    }
}
using System;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace SpaceKatMotionMapper.Views;

public partial class KatMotionGroupConfigWindow : UrsaWindow
{
    public const string LocalHost = "KatMotionGroupConfigWindow";
    private readonly WindowNotificationManager _manager;

    public KatMotionGroupConfigWindow()
    {
        InitializeComponent();
        _manager = new WindowNotificationManager(this)
        {
            MaxItems = 3,
            Position = NotificationPosition.TopCenter
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(DataContext))
        {
            WeakReferenceMessenger.Default
                .Register<PopupNotificationData, Guid>(this, (DataContext as KatMotionConfigViewModel)!.Id,
                    MessageHandler);
        }
    }
    
    private void MessageHandler(object? sender, PopupNotificationData e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var content = new Notification
            {
                Content = e.Message,
                ShowIcon = true,
                Title = e.NotificationType.ToString(),
                Type = e.NotificationType
            };
            _manager.Show(
                content);
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        WeakReferenceMessenger.Default
            .Unregister<PopupNotificationData, Guid>(this, (DataContext as KatMotionConfigViewModel)!.Id);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
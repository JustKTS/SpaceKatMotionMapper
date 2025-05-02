using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using MetaKeyPresetsEditor.Helpers;
using Microsoft.Extensions.DependencyInjection;
using MetaKeyPresetsEditor.ViewModels;
using SpaceKat.Shared.Models;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace MetaKeyPresetsEditor.Views;

public partial class PresetsEditorMainWindow : UrsaWindow
{
    private readonly WindowNotificationManager _manager;

    public const string LocalHost = "ProgramSpecificConfigWindow";

    public PresetsEditorMainWindow()
    {
        Content = DIHelper.GetServiceProvider().GetRequiredService<PresetsEditorMainView>();
        InitializeComponent();

        var topLevel = GetTopLevel(this);
        _manager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 3,
            Position = NotificationPosition.TopCenter
        };
    }


    public async Task PopupNotification(PopupNotificationData e)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _manager.Show(
                new Notification
                {
                    Content = e.Message,
                    ShowIcon = true,
                    Title = e.NotificationType.ToString(),
                    Type = e.NotificationType
                });
        });
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Content = null;
        base.OnClosing(e);
    }
}
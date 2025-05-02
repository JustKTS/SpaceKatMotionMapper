using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Services;

public class PopUpNotificationService
{
    public void Pop(NotificationType notificationType, string message)
    {
        WeakReferenceMessenger.Default.Send<PopupNotificationData, string>(new PopupNotificationData(notificationType, message), "PopUpNotification");
    }

    public void PopInKatMotionConfigWindow(Guid configId, NotificationType notificationType, string message)
    {
        WeakReferenceMessenger.Default.Send(new PopupNotificationData(notificationType, message), configId);

    }
}
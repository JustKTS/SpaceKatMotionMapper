using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class PopUpNotificationService : IPopUpNotificationService
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
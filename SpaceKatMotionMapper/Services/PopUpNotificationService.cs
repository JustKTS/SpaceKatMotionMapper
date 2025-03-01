using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace SpaceKatMotionMapper.Services;

public partial class PopUpNotificationService : ObservableRecipient
{
    public void Pop(NotificationType notificationType, string message)
    {
        WeakReferenceMessenger.Default.Send<PopupNotificationData, string>(new PopupNotificationData(notificationType, message), "PopUpNotification");
    }
}

public record PopupNotificationData(NotificationType NotificationType, string Message);
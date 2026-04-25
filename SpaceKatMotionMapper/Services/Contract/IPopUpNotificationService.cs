using System;
using Avalonia.Controls.Notifications;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IPopUpNotificationService
{
    void Pop(NotificationType notificationType, string message);
    void PopInKatMotionConfigWindow(Guid configId, NotificationType notificationType, string message);
}

using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using ProgramSpecificConfigCreator.Views;
using SpaceKat.Shared.Models;

namespace ProgramSpecificConfigCreator.Services;

public class PopUpNotificationService : IPopUpNotificationService
{
    private readonly ProgramSpecificConfigMainWindow
        _window = App.GetRequiredService<ProgramSpecificConfigMainWindow>();

    public async Task ShowPopUpNotificationAsync(PopupNotificationData popupData)
    {
        await _window.PopupNotification(popupData);
    }
}
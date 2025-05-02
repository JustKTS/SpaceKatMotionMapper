using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using SpaceKat.Shared.Models;

namespace MetaKeyPresetsEditor.Services;

public class PopUpNotificationSpecService : IPopUpNotificationSpecService
{
    private readonly PresetsEditorMainView
        _view = DIHelper.GetServiceProvider().GetRequiredService<PresetsEditorMainView>();

    public async Task ShowPopUpNotificationAsync(PopupNotificationData popupData)
    {
        var window = _view.GetLogicalParent() as PresetsEditorMainWindow;
        if (window is null) return;
        await window.PopupNotification(popupData);
    }
}
using System;
using System.Threading.Tasks;
using Avalonia.LogicalTree;
using MetaKeyPresetsEditor.Views;
using SpaceKat.Shared.Models;

namespace MetaKeyPresetsEditor.Services;

public class PopUpNotificationSpecService(
    Func<PresetsEditorMainView> viewFactory) : IPopUpNotificationSpecService
{
    public async Task ShowPopUpNotificationAsync(PopupNotificationData popupData)
    {
        var view = viewFactory();
        var window = view.GetLogicalParent() as PresetsEditorMainWindow;
        if (window is null) return;
        await window.PopupNotification(popupData);
    }
}

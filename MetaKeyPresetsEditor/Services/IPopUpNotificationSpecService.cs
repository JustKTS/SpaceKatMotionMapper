using System.Threading.Tasks;
using SpaceKat.Shared.Models;

namespace MetaKeyPresetsEditor.Services;

public interface IPopUpNotificationSpecService
{
    Task ShowPopUpNotificationAsync(PopupNotificationData popupData);
}
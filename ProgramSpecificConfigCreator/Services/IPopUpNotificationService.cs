using System.Threading.Tasks;
using SpaceKat.Shared.Models;

namespace ProgramSpecificConfigCreator.Services;

public interface IPopUpNotificationService
{
    Task ShowPopUpNotificationAsync(PopupNotificationData popupData);
}
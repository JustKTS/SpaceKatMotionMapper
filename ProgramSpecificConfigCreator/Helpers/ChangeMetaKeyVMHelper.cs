using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using ProgramSpecificConfigCreator.Services;
using ProgramSpecificConfigCreator.ViewModels;
using Serilog;
using SpaceKat.Shared.Models;

namespace ProgramSpecificConfigCreator.Helpers;

public static class ChangeMetaKeyVMHelper
{
    public static async Task LoadVMFromConfig(ProgramSpecMetaKeysRecord record)
    {
        await App.GetRequiredService<IUiInteractService>().ChangeConfigLoadingAsync(true);
        var vm = App.GetRequiredService<ProgramSpecificConfigViewModel>();

        var ret = await vm.LoadFromRecord(record);
        ret.IfFail(ex =>
        {
            App.GetRequiredService<IPopUpNotificationService>().ShowPopUpNotificationAsync(
                new PopupNotificationData(NotificationType.Error, $"加载配置文件失败，具体错误信息为：{ex.Message}"));
            Log.Logger.Error(ex, "");
        });

        await App.GetRequiredService<IUiInteractService>().ChangeConfigLoadingAsync(false);
    }
}
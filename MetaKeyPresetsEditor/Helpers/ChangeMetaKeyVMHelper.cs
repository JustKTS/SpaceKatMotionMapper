using System.Threading.Tasks;
using Avalonia.Controls.Notifications;

using MetaKeyPresetsEditor.Services;
using MetaKeyPresetsEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SpaceKat.Shared.Models;

namespace MetaKeyPresetsEditor.Helpers;

public static class ChangeMetaKeyVMHelper
{
    public static async Task LoadVMFromConfig(ProgramSpecMetaKeysRecord record)
    {
        await DIHelper.GetServiceProvider().GetRequiredService<IUiInteractService>().ChangeConfigLoadingAsync(true);
        var vm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();

        var ret = await vm.LoadFromRecord(record);
        if (ret.IsFailure)
        {
            await DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>().ShowPopUpNotificationAsync(
                new PopupNotificationData(NotificationType.Error, $"加载配置文件失败，具体错误信息为：{ret.Error.Message}"));
            Log.Logger.Error(ret.Error, "");
        }

        await DIHelper.GetServiceProvider().GetRequiredService<IUiInteractService>().ChangeConfigLoadingAsync(false);
    }
}
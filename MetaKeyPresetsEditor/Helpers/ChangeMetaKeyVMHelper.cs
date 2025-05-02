using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
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
        ret.IfFail(ex =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>().ShowPopUpNotificationAsync(
                new PopupNotificationData(NotificationType.Error, $"加载配置文件失败，具体错误信息为：{ex.Message}"));
            Log.Logger.Error(ex, "");
        });

        await DIHelper.GetServiceProvider().GetRequiredService<IUiInteractService>().ChangeConfigLoadingAsync(false);
    }
}
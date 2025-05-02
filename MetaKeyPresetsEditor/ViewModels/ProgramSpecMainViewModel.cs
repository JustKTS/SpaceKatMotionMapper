using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.Services;
using MetaKeyPresetsEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using SpaceKat.Shared.Defines;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.States;
using Ursa.Controls;

namespace MetaKeyPresetsEditor.ViewModels;

public partial class ProgramSpecMainViewModel : ViewModelBase
{
    private readonly IMetaKeyPresetFileService _metaKeyPresetFileService =
        DIHelper.GetServiceProvider().GetRequiredService<IMetaKeyPresetFileService>();


    private static readonly OverlayDialogOptions OverlayDialogOptions = new()
    {
        Buttons = DialogButton.None,
        CanDragMove = false,
        CanLightDismiss = true,
        CanResize = false,
        FullScreen = false,
        HorizontalAnchor = HorizontalPosition.Center,
        Mode = DialogMode.None,
        IsCloseButtonVisible = true
    };

    [RelayCommand]
    private static async Task OpenExistProgramSelector()
    {
        await OverlayDialog.ShowModal(DIHelper.GetServiceProvider().GetRequiredService<ExistPresetSelectorView>(),
            DIHelper.GetServiceProvider().GetRequiredService<ExistSpecConfigSelectorViewModel>(),
            PresetsEditorMainWindow.LocalHost,
            OverlayDialogOptions);
    }

    [RelayCommand]
    private void ModifyDefaultConfig()
    {
        var recordsRet = _metaKeyPresetFileService.LoadConfigs();
        var ret = recordsRet.Map(records =>
        {
            var vm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
            vm.IsDefault = true;

            if (records.TryGetValue("默认配置", out var record))
            {
                _ = vm.LoadFromRecord(record);
            }

            return true;
        });
        ret.IfLeft(ex =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Error, $"加载配置文件失败，{ex.Message}"));
        });
    }


    [RelayCommand]
    private static void AddNewConfig()
    {
        var vm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
        vm.ClearAll();
    }


    [RelayCommand]
    private static void OpenConfigFolder()
    {
        _ = Process.Start("explorer.exe", GlobalPaths.MetaKeysConfigPath);
    }

    [RelayCommand]
    private async Task GetPresetsFromInternet()
    {
        var ret = await DownloadMetaKeyPresetsHelper.DownloadAndCopyMetaKeyPresetsAsync();
        _ = ret.Match(s =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Success, "预设下载成功"));
            return true;
        }, ex =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Error, $"预设下载失败：{ex.Message}"));
            return false;
        });
    }

    private static FilePickerOpenOptions FileOpenOptions = new()
    {
        Title = "选择文件",
        AllowMultiple = false,
        FileTypeFilter = [FilePickerFileTypeDefines.Json]
    };

    [RelayCommand]
    private async Task GetPresetsFromFile()
    {
        var filePaths = await DIHelper.GetServiceProvider()
            .GetRequiredService<IStorageProvider>().OpenFilePickerAsync(FileOpenOptions);

        var ret = await DIHelper.GetServiceProvider()
            .GetRequiredService<IMetaKeyPresetFileService>().LoadFromFile(filePaths[0].Path.LocalPath).MapAsync(
                async record =>
                {
                    await ChangeMetaKeyVMHelper.LoadVMFromConfig(record);
                    return true;
                });
        ret.IfLeft(ex =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(new PopupNotificationData(NotificationType.Error, $"预设文件读取失败：{ex}"));
        });
    }
}
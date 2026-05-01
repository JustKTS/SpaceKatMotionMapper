using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.Services;
using MetaKeyPresetsEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using PlatformAbstractions;
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

    private readonly IFileExplorerService _fileExplorerService =
        DIHelper.GetServiceProvider().GetRequiredService<IFileExplorerService>();


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
        await OverlayDialog.ShowStandardAsync(DIHelper.GetServiceProvider().GetRequiredService<ExistPresetSelectorView>(),
            DIHelper.GetServiceProvider().GetRequiredService<ExistSpecConfigSelectorViewModel>(),
            PresetsEditorMainWindow.LocalHost,
            OverlayDialogOptions);
    }

    [RelayCommand]
    private void ModifyDefaultConfig()
    {
        var recordsRet = _metaKeyPresetFileService.LoadConfigs();
        if (recordsRet.IsSuccess)
        {
            var records = recordsRet.Value;
            var vm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
            vm.IsDefault = true;

            if (records.TryGetValue("默认配置", out var record))
            {
                _ = vm.LoadFromRecord(record);
            }
        }
        else
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Error, $"加载配置文件失败，{recordsRet.Error.Message}"));
        }
    }


    [RelayCommand]
    private static void AddNewConfig()
    {
        var vm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
        vm.ClearAll();
    }


    [RelayCommand]
    private void OpenConfigFolder()
    {
        _fileExplorerService.OpenPath(GlobalPaths.MetaKeysConfigPath);
    }

    [RelayCommand]
    private async Task GetPresetsFromInternet()
    {
        var ret = await DownloadMetaKeyPresetsHelper.DownloadAndCopyMetaKeyPresetsAsync();
        if (ret.IsSuccess)
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Success, "预设下载成功"));
        }
        else
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Error, $"预设下载失败：{ret.Error.Message}"));
        }
    }

    private static readonly FilePickerOpenOptions FileOpenOptions = new()
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

        var loadRet = DIHelper.GetServiceProvider()
            .GetRequiredService<IMetaKeyPresetFileService>().LoadFromFile(filePaths[0].Path.LocalPath);
        if (loadRet.IsSuccess)
        {
            await ChangeMetaKeyVMHelper.LoadVMFromConfig(loadRet.Value);
        }
        else
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                .ShowPopUpNotificationAsync(new PopupNotificationData(NotificationType.Error, $"预设文件读取失败：{loadRet.Error}"));
        }
    }
}
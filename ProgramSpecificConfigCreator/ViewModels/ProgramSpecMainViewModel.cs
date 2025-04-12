using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ProgramSpecificConfigCreator.Helpers;
using ProgramSpecificConfigCreator.Views;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.States;
using Ursa.Controls;

namespace ProgramSpecificConfigCreator.ViewModels;

public partial class ProgramSpecMainViewModel : ViewModelBase
{
    private readonly IProgramSpecMetaKeyFileService _metaKeyFileService =
        DIHelper.GetServiceProvider().GetRequiredService<IProgramSpecMetaKeyFileService>();

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
        await OverlayDialog.ShowModal(DIHelper.GetServiceProvider().GetRequiredService<ExistSpecConfigSelectorView>(),
            DIHelper.GetServiceProvider().GetRequiredService<ExistSpecConfigSelectorViewModel>(), ProgramSpecificConfigMainWindow.LocalHost,
            OverlayDialogOptions);
    }

    [RelayCommand]
    private void ModifyDefaultConfig()
    {
        var records = _metaKeyFileService.LoadConfigs();

        var vm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
        vm.IsDefault = true;

        if (records.TryGetValue("默认配置", out var record))
        {
            _ = vm.LoadFromRecord(record);
        }
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
}
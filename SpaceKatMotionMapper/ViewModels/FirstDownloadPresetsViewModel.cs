using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class FirstDownloadPresetsViewModel : ViewModelBase, IDialogContext
{
    private readonly MetaKeyPresetService _metaKeyPresetService = App.GetRequiredService<MetaKeyPresetService>();

    [RelayCommand]
    private async Task Download()
    {
        var ret = await DownloadMetaKeyPresetsHelper.DownloadAndCopyMetaKeyPresetsAsync();
        if (ret.IsSuccess)
        {
            _metaKeyPresetService.ReloadConfigs();
        }
        else
        {
            App.GetRequiredService<PopUpNotificationService>()
                .Pop(NotificationType.Error, $"预设下载失败：{ret.Error.Message}");
        }
        Close();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}
using Avalonia.Threading;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Functions;

public static class OnStartOrCloseFunctions
{
    public static void LoadOnStart()
    {
        var activateStatusService = App.GetRequiredService<ActivationStatusService>();
        activateStatusService.WaitForActivationStatusLoaded();
    }

    public static void LoadOnMainWindowLoaded()
    {
        var officialMapperHotKeyService = App.GetRequiredService<IOfficialMapperHotKeyService>();
        officialMapperHotKeyService.RegisterHandle();

        var settingsVm = App.GetRequiredService<SettingsViewModel>();
        settingsVm.LoadInStart();

        var connectVm = App.GetRequiredService<ConnectAndEnableViewModel>();
        connectVm.ConnectBtnCommand.Execute(null);
        App.GetRequiredService<ILocalSettingsService>()
            .ReadSettingAsync<bool>(GlobalStates.IsMapperEnableKey).ContinueWith(t =>
            {
                if (t.IsFaulted) return;
                var globalStates = App.GetRequiredService<GlobalStates>();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    globalStates.IsMapperEnable = t.Result;
                });
            });
    }

    public static void OnMainWindowClosing()
    {
        var officialMapperHotKeyService = App.GetRequiredService<IOfficialMapperHotKeyService>();
        officialMapperHotKeyService.UnregisterHotKeyWrapper();
    }
}
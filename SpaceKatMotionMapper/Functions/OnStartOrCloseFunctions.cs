using System;
using Avalonia.Threading;
using HidApi;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Functions;

public static class OnStartOrCloseFunctions
{
    public static void LoadOnStart()
    {
        Hid.Init();
        var activateStatusService = App.GetRequiredService<IActivationStatusService>();
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
                if (t.IsFaulted || t.IsCanceled) return;
                var globalStates = App.GetRequiredService<IGlobalStates>();
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
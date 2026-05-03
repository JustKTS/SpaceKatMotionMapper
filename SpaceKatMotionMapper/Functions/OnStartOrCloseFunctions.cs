using System;
using System.IO;
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
    private static readonly object _startupLogLock = new();
    private static void StartupLog(string msg)
    {
        var line = $"{DateTime.Now:HH:mm:ss.fff} [THREAD:{Environment.CurrentManagedThreadId}] {msg}\n";
        lock (_startupLogLock)
        {
            using var fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_debug.log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            fs.Write(System.Text.Encoding.UTF8.GetBytes(line));
            fs.Flush(false);
        }
    }

    public static void LoadOnStart()
    {
        StartupLog("LoadOnStart: BEFORE Hid.Init()");
        Hid.Init();
        StartupLog("LoadOnStart: AFTER Hid.Init(), BEFORE GetRequiredService");
        var activateStatusService = App.GetRequiredService<ActivationStatusService>();
        StartupLog("LoadOnStart: AFTER GetRequiredService, BEFORE WaitForActivationStatusLoaded");
        activateStatusService.WaitForActivationStatusLoaded();
        StartupLog("LoadOnStart: AFTER WaitForActivationStatusLoaded, DONE");
    }

    public static void LoadOnMainWindowLoaded()
    {
        StartupLog("LoadOnMainWindowLoaded: START");
        var officialMapperHotKeyService = App.GetRequiredService<IOfficialMapperHotKeyService>();
        StartupLog("LoadOnMainWindowLoaded: BEFORE RegisterHandle");
        officialMapperHotKeyService.RegisterHandle();
        StartupLog("LoadOnMainWindowLoaded: AFTER RegisterHandle, BEFORE LoadInStart");

        var settingsVm = App.GetRequiredService<SettingsViewModel>();
        settingsVm.LoadInStart();
        StartupLog("LoadOnMainWindowLoaded: AFTER LoadInStart, BEFORE ConnectBtnCommand");

        var connectVm = App.GetRequiredService<ConnectAndEnableViewModel>();
        StartupLog("LoadOnMainWindowLoaded: BEFORE ConnectBtnCommand.Execute");
        connectVm.ConnectBtnCommand.Execute(null);
        StartupLog("LoadOnMainWindowLoaded: AFTER ConnectBtnCommand.Execute");
        App.GetRequiredService<ILocalSettingsService>()
            .ReadSettingAsync<bool>(GlobalStates.IsMapperEnableKey).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled) return;
                var globalStates = App.GetRequiredService<GlobalStates>();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    globalStates.IsMapperEnable = t.Result;
                });
            });
        StartupLog("LoadOnMainWindowLoaded: DONE");
    }

    public static void OnMainWindowClosing()
    {
        var officialMapperHotKeyService = App.GetRequiredService<IOfficialMapperHotKeyService>();
        officialMapperHotKeyService.UnregisterHotKeyWrapper();
    }
}
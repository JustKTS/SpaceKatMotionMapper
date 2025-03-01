using System;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HidApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;
using Win32Helpers;
using WindowsInput;

namespace SpaceKatMotionMapper;

public partial class App : Application
{
    private IHost Host { get; }

    public static T GetService<T>()
        where T : class
    {
        try
        {
            return (App.Current as App)!.Host.Services.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    public static T GetRequiredService<T>()
        where T : class
    {
        try
        {
            return (App.Current as App)!.Host.Services.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    public App()
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();

        Host = Microsoft
            .Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainView>();
                services.AddSingleton<MainViewModel>();

                services.AddSingleton<ListeningInfoViewModel>();

                services.AddSingleton<ConfigCenterViewModel>();

                services.AddSingleton<IDeviceDataWrapper, SpaceCompatDataWrapper>();
                services.AddSingleton<KatActionRecognizeService>();

                services.AddSingleton<KatMotionTimeConfigService>();
                services.AddSingleton<KatDeadZoneConfigService>();
                services.AddSingleton<TimeAndDeadZoneSettingViewModel>();
                services.AddSingleton<KatMotionTimeConfigView>();
                services.AddSingleton<MotionTimeConfigViewModel>();
                services.AddSingleton<DeadZoneConfigView>();
                services.AddSingleton<DeadZoneConfigViewModel>();
                services.AddSingleton<TimeAndDeadZoneVMService>();


                services.AddSingleton<ITopLevelHelper, TopLevelHelper>();
                services.AddSingleton<IStorageProviderService, StorageProviderService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<CurrentForeProgramHelper>();
                services.AddSingleton<ActivationStatusService>();
                services.AddSingleton<InputSimulator>();
                services.AddSingleton<KatActionActivateService>();
                services.AddSingleton<KatActionFileService>();
                services.AddSingleton<PopUpNotificationService>();
                services.AddSingleton<CommonConfigViewModel>();
                services.AddTransient<KatActionConfigViewModel>();
                services.AddSingleton<OtherConfigsViewModel>();
                services.AddTransient<CurrentRunningProcessSelectorViewModel>();
                services.AddSingleton<ModeChangeService>();
                services.AddSingleton<ConflictKatActionService>();
                services.AddSingleton<KatActionConfigVMManageService>();
                services.AddSingleton<OfficialMapperSwitchService>();
            })
            .Build();
    }

    public override void Initialize()
    {
        var activateStatusService = GetService<ActivationStatusService>();
        activateStatusService.WaitForActivationStatusLoaded();
        DataContext = this;
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var ret = SetSingleton();
                if (!ret)
                {   
                    var wrongWindow = new SingletonWrongWindow();
                    wrongWindow.Show();
                    desktop.Shutdown();
                    return;
                }
                var mainWindow = GetService<MainWindow>();
                desktop.MainWindow = mainWindow;
                mainWindow.Closed += (_, _) =>
                {
                    GetService<ActivationStatusService>().SaveActivationStatus();
                    CloseApp();
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void CloseApp()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var ofMs =
                    GetRequiredService<OfficialMapperSwitchService>();
                ofMs.UnregisterHotKeyWrapper();
                ofMs.UnregisterHandleFunc();
                OfficialMapperSwitchService.CleanAllChange();

                Hid.Exit();
                _mutex?.WaitOne();
                _mutex?.ReleaseMutex();
                desktop.Shutdown();
                break;
        }
    }

    private void ExitMenuItem_OnClick(object? sender, EventArgs e)
    {
        CloseApp();
    }

    private void ShowWindowMenuItem_OnClick(object? sender, EventArgs e)
    {
        var window = GetService<MainWindow>();
        window.WindowState = WindowState.Normal;
        window.ShowInTaskbar = true;
    }
    
    private static Mutex? _mutex;
    private static bool SetSingleton()
    {
        _mutex = new Mutex(true, "SpaceMotionMapper", out var ret);
        return ret;
    }
}
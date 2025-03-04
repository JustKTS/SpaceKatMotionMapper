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
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Serilog;
using Serilog.Events;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using Win32Helpers;
using WindowsInput;
using ILogger = Serilog.ILogger;
using Path = System.IO.Path;

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
                services.AddSingleton<TransparentInfoWindow>();
                services.AddSingleton<TransparentInfoViewModel>();
                
                services.AddSingleton<MainView>();
                services.AddSingleton<MainViewModel>();

                services.AddSingleton<ListeningInfoViewModel>();
                services.AddSingleton<AutoDisableViewModel>();

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
                services.AddSingleton<AutoDisableService>();
                
                services.AddSingleton<PopUpNotificationService>();
                services.AddSingleton<TransparentInfoService>();

                services.AddSingleton<ITopLevelHelper, TopLevelHelper>();
                services.AddSingleton<IStorageProviderService, StorageProviderService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<CurrentForeProgramHelper>();
                services.AddSingleton<ActivationStatusService>();
                services.AddSingleton<InputSimulator>();
                services.AddSingleton<KatActionActivateService>();
                services.AddSingleton<KatActionFileService>();
                services.AddSingleton<CommonConfigViewModel>();
                services.AddTransient<KatActionConfigViewModel>();
                services.AddSingleton<OtherConfigsViewModel>();
                services.AddTransient<CurrentRunningProcessSelectorViewModel>();
                services.AddSingleton<ModeChangeService>();
                services.AddSingleton<ConflictKatActionService>();
                services.AddSingleton<KatActionConfigVMManageService>();
                services.AddSingleton<OfficialMapperSwitchService>();
            })
            .UseSerilog()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    nameof(SpaceKatMotionMapper), "Log.log");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.File(logPath,rollingInterval: RollingInterval.Day)
                    // .WriteTo.Trace(
                    //     outputTemplate:
                    //     "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .MinimumLevel.Information()
                    .CreateLogger();
                logging.Services.AddSingleton<ILogger>(Log.Logger);
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
                    desktop.MainWindow = wrongWindow;
                    desktop.MainWindow.Closed += (_, _) => { desktop.Shutdown(); };
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
                ofMs.UnregisterHandle();
                OfficialWareConfigFunctions.CleanAllChange();

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
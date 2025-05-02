using System;
using System.Diagnostics;
using System.IO;
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
using MetaKeyPresetsEditor.Helpers;
using Serilog;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
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
using SpaceKat.Shared.States;
using SpaceKat.Shared.ViewModels;
using SpaceKatMotionMapper.NavVMs;
using SpaceKatMotionMapper.States;

namespace SpaceKatMotionMapper;

public class App : Application
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
            return (Current as App)!.Host.Services.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    public static object GetRequiredView(Type type)
    {
        try
        {
            return (Current as App)!.Host.Services.GetRequiredService(type);
        }
        catch (Exception ex)
        {
            GetRequiredService<ILogger>().Fatal(ex, "");
            throw;
        }
    }

    public App()
    {
        Host = Microsoft
            .Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<NavViewModel>();
                services.AddSingleton<ViewRegister>();

                services.AddTransient<KatMotionGroupConfigWindow>();

                services.AddTransient<TransparentInfoWindow>();
                services.AddSingleton<TransparentInfoViewModel>();
                
                services.AddTransient<FavPresetsEditorView>();
                services.AddTransient<FavPresetsEditorViewModel>();
                services.AddTransient<FirstDownloadPresetsView>();
                services.AddTransient<FirstDownloadPresetsViewModel>();

                services.AddSingleton<MainView>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SettingsView>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<ListeningInfoViewModel>();
                services.AddSingleton<ConnectAndEnableViewModel>();
                services.AddSingleton<AutoDisableViewModel>();
                services.AddSingleton<RunningProgramSelectorViewModel>();

                services.AddSingleton<IDeviceDataWrapper, SpaceCompatDataWrapper>();
                services.AddSingleton<KatMotionRecognizeService>();
                services.AddSingleton<TransparentInfoActionDisplayService>();

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
                services.AddSingleton<GlobalStates>();

                services.AddSingleton<IStorageProviderService, StorageProviderService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<CurrentForeProgramHelper>();
                services.AddSingleton<ActivationStatusService>();
                services.AddSingleton<InputSimulator>();
                services.AddSingleton<KatMotionActivateService>();
                services.AddSingleton<KatMotionFileService>();
                services.AddSingleton<CommonConfigViewModel>();
                services.AddTransient<KatMotionConfigViewModel>();
                services.AddSingleton<OtherConfigsViewModel>();
                services.AddSingleton<ModeChangeService>();
                services.AddSingleton<ConflictKatMotionService>();
                services.AddSingleton<KatMotionConfigVMManageService>();
                services.AddSingleton<IOfficialMapperHotKeyService, OfficialMapperHotKeyService>();
                services.AddSingleton<MetaKeyPresetService>();
                services.AddSingleton<MetaKeyPresetFileService>();

                // 分应用快捷键预设配置工具
                DIHelper.RegisterServices(services);
            })
            .UseSerilog()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();

                if (!Directory.Exists(GlobalPaths.AppLogPath))
                {
                    Directory.CreateDirectory(GlobalPaths.AppLogPath);
                }

                var logPath = Path.Combine(GlobalPaths.AppLogPath, "Log.log");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                    .MinimumLevel.Information()
                    .CreateLogger();
                logging.Services.AddSingleton(Log.Logger);
            })
            .Build();
        DIHelper.SetServiceProvider(Host.Services);
    }

    // [AvaloniaHotReload]
    public override void Initialize()
    {
        OnStartOrCloseFunctions.LoadOnStart();
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
                // var mainWindow = new TestWindow();
                desktop.MainWindow = mainWindow;
                mainWindow.Closed += (_, _) => { CloseApp(); };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void CloseApp()
    {
        GetService<ActivationStatusService>().SaveActivationStatus();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var ofMs =
                    GetRequiredService<IOfficialMapperHotKeyService>();
                ofMs.UnregisterHotKeyWrapper();
                ofMs.UnregisterHandle();
                OfficialWareConfigFunctions.CleanAllChange().GetAwaiter().GetResult();
                Hid.Exit();
                GetRequiredService<CurrentForeProgramHelper>().Dispose();
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
using System;
using System.IO;
using Avalonia;
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
using SpaceKatHIDWrapper.DeviceHIDSpecs;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using PlatformAbstractions;
using Serilog.Sinks.OpenTelemetry;
using SpaceKat.Shared.Logging;

#if WINDOWS
using Win32Helpers;
#elif LINUX
using LinuxHelpers;
#endif

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
            return (Current as App)!.Host.Services.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{App}] GetService failed for {ServiceType}", nameof(App), typeof(T).Name);
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
            Log.Error(ex, "[{App}] GetRequiredService failed for {ServiceType}", nameof(App), typeof(T).Name);
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

                services.AddSingleton<IDeviceDataWrapper, SpaceDeviceDataWrapper>();
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

                // 核心服务接口注册
                services.AddSingleton<IKatMotionFileService, KatMotionFileService>();
                services.AddSingleton<IPopUpNotificationService, PopUpNotificationService>();
                services.AddSingleton<IKatMotionActivateService, KatMotionActivateService>();
                services.AddSingleton<IActivationStatusService, ActivationStatusService>();
                services.AddSingleton<IKatMotionConfigVMManageService, KatMotionConfigVMManageService>();

                // 平台特定服务
#if LINUX
                services.AddLinuxPlatformServices();
                services.AddSingleton<IFloatingControlWindowUIFactory>(_ =>
                    new LinuxHelpers.Services.FloatingWindow.FloatingControlWindowUIFactory(
                        () => new FloatingControlWindow()));
                services.AddTransient<FloatingControlWindow>();
#elif WINDOWS
                services.AddWindowsPlatformServices();
#else
                services.AddSingleton<IPlatformWindowService, PlatformAbstractions.Unsupported.UnsupportedPlatformWindowService>();
                services.AddSingleton<IPlatformHotKeyService, PlatformAbstractions.Unsupported.UnsupportedPlatformHotKeyService>();
                services.AddSingleton<IPlatformForegroundProgramService, PlatformAbstractions.Unsupported.UnsupportedPlatformForegroundProgramService>();
                services.AddSingleton<IPlatformMinimizeService, GenericPlatformMinimizeService>();
                services.AddSingleton<IFileExplorerService, PlatformAbstractions.Unsupported.UnsupportedFileExplorerService>();
                services.AddSingleton<ISingletonInstanceService, PlatformAbstractions.Unsupported.UnsupportedSingletonInstanceService>();
#endif
                services.AddSingleton<ActivationStatusService>();
                // 保留具体类注册以向后兼容，同时优先使用接口
                services.AddSingleton<KatMotionActivateService>();
                services.AddSingleton<KatMotionFileService>();
                services.AddSingleton<CommonConfigViewModel>();
                services.AddTransient<KatMotionConfigViewModel>();
                services.AddSingleton<OtherConfigsViewModel>();
                services.AddSingleton<ModeChangeService>();
                services.AddSingleton<ConflictKatMotionService>();
                // Ensure interface and concrete resolve to the same singleton instance.
                services.AddSingleton(sp => (KatMotionConfigVMManageService)sp.GetRequiredService<IKatMotionConfigVMManageService>());
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

                // 生成实例ID并创建enricher
                var instanceId = Guid.NewGuid().ToString();
                var instanceIdEnricher = new InstanceIdEnricher(instanceId);

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .Enrich.With(instanceIdEnricher)  // 添加实例ID enricher
#if DEBUG
                    .WriteTo.OpenTelemetry("http://localhost:9428/insert/opentelemetry/v1/logs", OtlpProtocol.HttpProtobuf)
#endif
                    .WriteTo.File(logPath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [InstanceId:{InstanceId}] {Message:lj}{NewLine}{Exception}")
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
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var singleton = GetRequiredService<ISingletonInstanceService>();
                if (!singleton.TryAcquire())
                {
                    var wrongWindow = new SingletonWrongWindow();
                    desktop.MainWindow = wrongWindow;
                    desktop.MainWindow.Closed += (_, _) => { desktop.Shutdown(); };
                    return;
                }

                try
                {
                    DeviceHidSpecDict.Initialize(GlobalPaths.AppDataPath);
                }
                catch (Exception ex)
                {
                    var recoveryWindow = new ConfigRecoveryWindow(ex.Message);
                    desktop.MainWindow = recoveryWindow;
                    recoveryWindow.Closed += (_, _) =>
                    {
                        if (recoveryWindow.ShouldReset)
                        {
                            try
                            {
                                DeviceHidSpecDict.ResetToDefault(GlobalPaths.AppDataPath);
                                var mainWindow = GetService<MainWindow>();
                                desktop.MainWindow = mainWindow;
                                mainWindow.Show();
                                mainWindow.Closed += (_, _) => { CloseApp(); };
                            }
                            catch
                            {
                                desktop.Shutdown();
                            }
                        }
                        else
                        {
                            desktop.Shutdown();
                        }
                    };
                    return;
                }

                var mainWindow2 = GetService<MainWindow>();
                // var mainWindow2 = new TestWindow();
                desktop.MainWindow = mainWindow2;
                mainWindow2.Closed += (_, _) => { CloseApp(); };
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
                var foregroundService = GetRequiredService<IPlatformForegroundProgramService>();
                if (foregroundService is IDisposable disposableForegroundService)
                {
                    disposableForegroundService.Dispose();
                }

                var minimizeService = GetRequiredService<IPlatformMinimizeService>();
                minimizeService.Dispose();

                #if WINDOWS
                if (GetRequiredService<CurrentForeProgramHelper>() is { } helper)
                {
                    helper.Dispose();
                }
                #endif
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
        var minimizeService = GetRequiredService<IPlatformMinimizeService>();
        minimizeService.RestoreWindow(window);
    }

}
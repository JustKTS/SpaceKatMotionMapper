using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HidApi;
using Serilog;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Logging;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.States;
using SpaceKatHIDWrapper.DeviceHIDSpecs;
using SpaceKatMotionMapper.Composition;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Views;
using PlatformAbstractions;

using ILogger = Serilog.ILogger;
using Path = System.IO.Path;

namespace SpaceKatMotionMapper;

public class App : Application
{
    public static SpaceKatServiceProvider Container { get; private set; } = null!;

    public static T GetService<T>() where T : class
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return Container.GetService<T>()!;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetService failed for {ServiceType}", typeof(T).Name);
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }

    public static T GetRequiredService<T>() where T : class
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return Container.GetService<T>()!;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetRequiredService failed for {ServiceType}", typeof(T).Name);
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }

    public static object GetService(Type type)
    {
        var sw = Stopwatch.StartNew();
        var result = ((IServiceProvider)Container).GetService(type)!;
        sw.Stop();
        return result;
    }

    private static readonly ILogger Log = Serilog.Log.ForContext<App>();

    public App()
    {
        GlobalPaths.Initialize("SpaceKatMotionMapper");

        if (!Directory.Exists(GlobalPaths.AppLogPath))
            Directory.CreateDirectory(GlobalPaths.AppLogPath);

        var logPath = Path.Combine(GlobalPaths.AppLogPath, "Log.log");
        var instanceId = Guid.NewGuid().ToString();
        var instanceIdEnricher = new InstanceIdEnricher(instanceId);

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.With(instanceIdEnricher)
#if DEBUG
            .WriteTo.OpenTelemetry("http://localhost:9428/insert/opentelemetry/v1/logs", Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf)
#endif
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [InstanceId:{InstanceId}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var provider = new SpaceKatServiceProvider();
        provider.Logger = Serilog.Log.Logger;
        Container = provider;
        MetaKeyPresetsEditor.App.FallbackProvider = provider;

    }

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
                var singleton = Container.GetService<ISingletonInstanceService>()!;
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
                                var mainWindow = Container.GetService<MainWindow>();
                                desktop.MainWindow = mainWindow;
                                mainWindow!.Show();
                                mainWindow.Closed += (_, _) => { CloseApp(); };
                            }
                            catch { desktop.Shutdown(); }
                        }
                        else { desktop.Shutdown(); }
                    };
                    return;
                }

                var mainWindow2 = Container.GetService<MainWindow>()!;
                desktop.MainWindow = mainWindow2;
                mainWindow2.Closed += (_, _) => { CloseApp(); };
                break;
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void CloseApp()
    {
        var activationStatus = Container.GetService<IActivationStatusService>()!;
        activationStatus.SaveActivationStatus();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var ofms = Container.GetService<IOfficialMapperHotKeyService>()!;
                ofms.UnregisterHotKeyWrapper();
                ofms.UnregisterHandle();
                OfficialWareConfigFunctions.CleanAllChange().GetAwaiter().GetResult();
                Hid.Exit();
                var foregroundService = Container.GetService<IPlatformForegroundProgramService>()!;
                if (foregroundService is IDisposable disposableForegroundService)
                    disposableForegroundService.Dispose();
                var minimizeService = Container.GetService<IPlatformMinimizeService>()!;
                minimizeService.Dispose();
                desktop.Shutdown();
                break;
        }
        Serilog.Log.CloseAndFlush();
    }

    private void ExitMenuItem_OnClick(object? sender, EventArgs e) => CloseApp();

    private void ShowWindowMenuItem_OnClick(object? sender, EventArgs e)
    {
        var window = Container.GetService<MainWindow>();
        var minimizeService = Container.GetService<IPlatformMinimizeService>()!;
        minimizeService.RestoreWindow(window!);
    }
}

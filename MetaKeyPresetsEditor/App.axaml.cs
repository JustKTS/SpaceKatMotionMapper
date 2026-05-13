using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MetaKeyPresetsEditor.Views;
using Serilog;
using SpaceKat.Shared.States;

namespace MetaKeyPresetsEditor;

public class App : Application
{
    private static readonly ILogger Log = Serilog.Log.ForContext<App>();

    public static MetaKeyPresetsServiceProvider Container { get; private set; } = null!;
    public static IServiceProvider? FallbackProvider { get; set; }

    public static IStorageProvider GetStorageProvider()
    {
        var mainView = GetRequiredService<PresetsEditorMainView>();
        var toplevel = Avalonia.Controls.TopLevel.GetTopLevel(mainView);
        return toplevel!.StorageProvider;
    }

    public static T GetRequiredService<T>() where T : class
    {
        try
        {
            if (Container != null)
            {
                if (typeof(T) == typeof(IStorageProvider))
                {
                    var mainView = Container.GetService<PresetsEditorMainView>();
                    if (mainView != null)
                    {
                        var topLevel = TopLevel.GetTopLevel(mainView);
                        if (topLevel?.StorageProvider != null)
                            return (T)topLevel.StorageProvider;
                    }
                }
                return Container.GetService<T>()!;
            }

            if (FallbackProvider != null)
                return (T)FallbackProvider.GetService(typeof(T))!;

            throw new InvalidOperationException("No DI container available.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetRequiredService failed for {ServiceType}", typeof(T).Name);
            throw;
        }
    }

    public App()
    {
        if (!Directory.Exists(GlobalPaths.AppLogPath))
            Directory.CreateDirectory(GlobalPaths.AppLogPath);

        var logPath = Path.Combine(GlobalPaths.AppLogPath, "Log.log");
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var provider = new MetaKeyPresetsServiceProvider();
        provider.Logger = Serilog.Log.Logger;
        Container = provider;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = GetRequiredService<PresetsEditorMainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

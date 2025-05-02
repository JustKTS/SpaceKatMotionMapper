using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SpaceKat.Shared.States;

namespace MetaKeyPresetsEditor;

public class App : Application
{
    private IHost Host { get; }

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

    public App()
    {
        Host = Microsoft
            .Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(DIHelper.RegisterServices)
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

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = GetRequiredService<PresetsEditorMainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
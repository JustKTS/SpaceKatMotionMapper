using Avalonia;
using System;
using Serilog;
using SpaceKat.Shared.Functions;
#if LINUX
using LinuxHelpers.Services.Platform;
#endif

namespace SpaceKatMotionMapper;

sealed class Program
{
    public static bool ShouldMinimizeToTray { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ShouldMinimizeToTray = Array.Exists(args, a => a == "--minimized");

#if LINUX
        LinuxScaleDetector.DetectAndApply();
#endif
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application crashed");
            OfficialWareConfigFunctions.CleanAllChange().GetAwaiter().GetResult();
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .LogToTrace()
#endif
            ;
}

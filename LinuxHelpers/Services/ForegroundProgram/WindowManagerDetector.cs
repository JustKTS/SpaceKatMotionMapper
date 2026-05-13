using System.Diagnostics;
using Serilog;

namespace LinuxHelpers.Services.ForegroundProgram;

public enum WindowManagerType
{
    Unknown,
    Niri,
    Other
}

public static class WindowManagerDetector
{
    private static readonly ILogger Log = Serilog.Log.ForContext("SourceContext", nameof(WindowManagerDetector));

    private static WindowManagerType? _cachedType;

    public static WindowManagerType DetectWindowManager()
    {
        if (_cachedType.HasValue)
            return _cachedType.Value;

        // Check XDG_CURRENT_DESKTOP for niri
        var currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        if (!string.IsNullOrEmpty(currentDesktop))
        {
            var desktops = currentDesktop.Split([':', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var desktop in desktops)
            {
                if (desktop.Contains("niri", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("Detected Niri via XDG_CURRENT_DESKTOP");
                    return CacheResult(WindowManagerType.Niri);
                }
            }
        }

        // Check for niri process
        if (IsProcessRunning("niri"))
        {
            Log.Information("Detected Niri via running process");
            return CacheResult(WindowManagerType.Niri);
        }

        return CacheResult(WindowManagerType.Other);
    }

    public static bool IsNiri() => DetectWindowManager() == WindowManagerType.Niri;

    private static WindowManagerType CacheResult(WindowManagerType type)
    {
        _cachedType = type;
        return type;
    }

    private static bool IsProcessRunning(string processName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "pgrep",
                Arguments = $"-xf \"{processName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception checking process {ProcessName}", processName);
            return false;
        }
    }
}

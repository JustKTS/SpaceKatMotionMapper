using System.Diagnostics;
using Serilog;

namespace LinuxHelpers.Services.ForegroundProgram;

/// <summary>
/// 窗口管理器类型枚举
/// </summary>
public enum WindowManagerType
{
    Unknown,
    Niri,
    Hyprland,
    Kde,
    Sway,
    Other
}

/// <summary>
/// 窗口管理器检测器
/// 通过多种策略检测当前运行的窗口管理器类型
/// </summary>
public static class WindowManagerDetector
{
    private static WindowManagerType? _cachedType;

    /// <summary>
    /// 检测当前运行的窗口管理器类型
    /// 结果会被缓存，避免重复检测
    /// </summary>
    /// <returns>窗口管理器类型</returns>
    public static WindowManagerType DetectWindowManager()
    {
        if (_cachedType.HasValue)
        {
            Log.Debug("[{Service}] Returning cached type: {Type}", nameof(WindowManagerDetector), _cachedType.Value);
            return _cachedType.Value;
        }

        Log.Debug("[{Service}] Starting window manager detection", nameof(WindowManagerDetector));

        // 策略1: 检查 XDG_SESSION_TYPE
        var sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
        Log.Debug("[{Service}] XDG_SESSION_TYPE: {SessionType}", nameof(WindowManagerDetector), sessionType);

        if (sessionType?.ToLower() != "wayland")
        {
            Log.Information("[{Service}] Not running on Wayland", nameof(WindowManagerDetector));
            return CacheResult(WindowManagerType.Unknown);
        }

        // 策略2: 检查 XDG_CURRENT_DESKTOP
        var currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        Log.Debug("[{Service}] XDG_CURRENT_DESKTOP: {Desktop}", nameof(WindowManagerDetector), currentDesktop);

        if (!string.IsNullOrEmpty(currentDesktop))
        {
            // 某些环境可能包含多个桌面环境，用冒号或分号分隔
            var desktops = currentDesktop.Split([':', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var desktop in desktops)
            {
                if (desktop.Contains("niri", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("[{Service}] Detected Niri via XDG_CURRENT_DESKTOP", nameof(WindowManagerDetector));
                    return CacheResult(WindowManagerType.Niri);
                }

                if (desktop.Contains("hyprland", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("[{Service}] Detected Hyprland via XDG_CURRENT_DESKTOP", nameof(WindowManagerDetector));
                    return CacheResult(WindowManagerType.Hyprland);
                }

                if (desktop.Contains("kde", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("[{Service}] Detected KDE via XDG_CURRENT_DESKTOP", nameof(WindowManagerDetector));
                    return CacheResult(WindowManagerType.Kde);
                }

                if (desktop.Contains("sway", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("[{Service}] Detected Sway via XDG_CURRENT_DESKTOP", nameof(WindowManagerDetector));
                    return CacheResult(WindowManagerType.Sway);
                }
            }
        }

        // 策略3: 检查正在运行的进程
        Log.Debug("[{Service}] Checking running processes...", nameof(WindowManagerDetector));

        if (IsProcessRunning("niri"))
        {
            Log.Information("[{Service}] Detected Niri via running process", nameof(WindowManagerDetector));
            return CacheResult(WindowManagerType.Niri);
        }

        if (IsProcessRunning("Hyprland"))
        {
            Log.Information("[{Service}] Detected Hyprland via running process", nameof(WindowManagerDetector));
            return CacheResult(WindowManagerType.Hyprland);
        }

        if (IsProcessRunning("plasmashell") || IsProcessRunning("kwin"))
        {
            Log.Information("[{Service}] Detected KDE via running process", nameof(WindowManagerDetector));
            return CacheResult(WindowManagerType.Kde);
        }

        if (IsProcessRunning("sway"))
        {
            Log.Information("[{Service}] Detected Sway via running process", nameof(WindowManagerDetector));
            return CacheResult(WindowManagerType.Sway);
        }

        Log.Warning("[{Service}] No recognized window manager found", nameof(WindowManagerDetector));
        return CacheResult(WindowManagerType.Other);
    }

    /// <summary>
    /// 缓存检测结果
    /// </summary>
    private static WindowManagerType CacheResult(WindowManagerType type)
    {
        _cachedType = type;
        return type;
    }

    /// <summary>
    /// 检查指定进程是否正在运行
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <returns>如果进程正在运行返回 true，否则返回 false</returns>
    private static bool IsProcessRunning(string processName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "pgrep",
                Arguments = $"-x \"{processName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                Log.Warning("[{Service}] Failed to start pgrep for {ProcessName}", nameof(WindowManagerDetector), processName);
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit(1000);

            var isRunning = process.ExitCode == 0;
            Log.Debug("[{Service}] Process check for {ProcessName}: {IsRunning} (output: '{Output}', error: '{Error}')",
                nameof(WindowManagerDetector), processName, isRunning, output.Trim(), error.Trim());

            return isRunning;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Service}] Exception checking process {ProcessName}: {Message}",
                nameof(WindowManagerDetector), processName, ex.Message);
            return false;
        }
    }
}

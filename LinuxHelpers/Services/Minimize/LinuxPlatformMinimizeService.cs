using Avalonia.Controls;
using PlatformAbstractions;

namespace LinuxHelpers.Services.Minimize;

/// <summary>
/// Linux平台最小化服务实现
/// 支持系统托盘检测和隐藏窗口功能
/// </summary>
public class LinuxPlatformMinimizeService : IPlatformMinimizeService
{
    private Avalonia.Controls.Window? _currentWindow;
    private WindowState _previousState;
    private readonly IFloatingControlWindowService _floatingWindowService;

    public bool IsSupported => true;

    public bool CanMinimizeToTray { get; }

    public bool CanHideWindow => true;

    public event EventHandler<object>? WindowMinimized;
    public event EventHandler<object>? WindowRestored;
    public event EventHandler<object>? WindowHidden;

    public LinuxPlatformMinimizeService(IFloatingControlWindowService floatingWindowService)
    {
        // 检测Linux桌面环境是否支持系统托盘
        CanMinimizeToTray = DetectTraySupport();
        _floatingWindowService = floatingWindowService;

        // 订阅浮动窗口服务事件
        _floatingWindowService.MainWindowRestoreRequested += OnMainWindowRestoreRequested;
    }

    public void SetWindowRef(Avalonia.Controls.Window? window)
    {
        _currentWindow = window;
    }

    public void MinimizeWindow(object window)
    {
        if (window is not Avalonia.Controls.Window avaloniaWindow) return;
        _currentWindow = avaloniaWindow;
        _previousState = avaloniaWindow.WindowState;

        if (IsNiriWindowManager())
        {
            // Niri窗口管理器特殊处理：直接隐藏窗口
            HideToBackground(avaloniaWindow);

            // 显示浮动窗口
            ShowFloatingWindowIfNeeded();
        }
        else if (CanMinimizeToTray)
        {
            // 支持托盘的环境：最小化并隐藏
            avaloniaWindow.WindowState = WindowState.Minimized;
            avaloniaWindow.ShowInTaskbar = false;
            WindowMinimized?.Invoke(this, avaloniaWindow);
        }
        else
        {
            // 不支持托盘：最小化到任务栏，同时显示浮动窗口
            avaloniaWindow.WindowState = WindowState.Minimized;
            avaloniaWindow.ShowInTaskbar = false;
            WindowMinimized?.Invoke(this, avaloniaWindow);

            // 显示浮动窗口
            ShowFloatingWindowIfNeeded();
        }
    }

    private void ShowFloatingWindowIfNeeded()
    {
        // 只有在不支持托盘且有浮动窗口服务时才显示浮动窗口
        if (!CanMinimizeToTray && _floatingWindowService.IsSupported)
        {
            _floatingWindowService.Show(_currentWindow!);
        }
    }

    public void RestoreWindow(object window)
    {
        if (window is not Avalonia.Controls.Window avaloniaWindow) return;

        avaloniaWindow.ShowInTaskbar = true;
        avaloniaWindow.WindowState = WindowState.Normal;
        avaloniaWindow.Show();
        avaloniaWindow.Activate();

        // 隐藏浮动窗口
        HideFloatingWindowIfNeeded();

        WindowRestored?.Invoke(this, avaloniaWindow);
    }

    private void HideFloatingWindowIfNeeded()
    {
        // 隐藏浮动窗口
        if (_floatingWindowService.IsVisible)
        {
            _floatingWindowService.Hide();
        }
    }

    public void HideToBackground(object window)
    {
        if (window is not Avalonia.Controls.Window avaloniaWindow) return;
        _currentWindow = avaloniaWindow;
        _previousState = avaloniaWindow.WindowState;

        // 对于Niri或其他不兼容ShowInTaskbar的窗口管理器
        // 直接隐藏窗口
        avaloniaWindow.Hide();
        WindowHidden?.Invoke(this, avaloniaWindow);
    }

    /// <summary>
    /// 检测当前是否运行在Niri窗口管理器上
    /// </summary>
    private static bool IsNiriWindowManager()
    {
        try
        {
            // 检查XDG_CURRENT_DESKTOP环境变量
            var xdgDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
            if (!string.IsNullOrEmpty(xdgDesktop))
            {
                if (xdgDesktop.Contains("niri", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // 检查WAYLAND_DISPLAY（Niri主要支持Wayland）
            var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            if (!string.IsNullOrEmpty(waylandDisplay))
            {
                // 在Wayland环境下，进一步检查窗口管理器
                return IsNiriRunning();
            }
        }
        catch
        {
            // 检测失败时假设不是Niri
        }

        return false;
    }

    /// <summary>
    /// 检测Niri是否正在运行
    /// </summary>
    private static bool IsNiriRunning()
    {
        try
        {
            // 尝试通过环境变量或进程检测Niri
            var session = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
            if (session == "wayland")
            {
                // 在Wayland环境下，假设可能是Niri（简化检测）
                return true;
            }
        }
        catch
        {
            // 检测失败
        }

        return false;
    }

    /// <summary>
    /// 检测当前桌面环境是否支持系统托盘
    /// </summary>
    private static bool DetectTraySupport()
    {
        try
        {
            var xdgDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
            if (string.IsNullOrEmpty(xdgDesktop))
                return false;

            var desktop = xdgDesktop.ToLowerInvariant();

            // 支持系统托盘的桌面环境
            return desktop.Contains("gnome") ||
                   desktop.Contains("kde") ||
                   desktop.Contains("plasma") ||
                   desktop.Contains("xfce") ||
                   desktop.Contains("lxqt") ||
                   desktop.Contains("mate") ||
                   desktop.Contains("cinnamon") ||
                   desktop.Contains("niri") ||
                   desktop.Contains("budgie");
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _currentWindow = null;

        // 取消订阅浮动窗口服务事件
        _floatingWindowService.MainWindowRestoreRequested -= OnMainWindowRestoreRequested;

        GC.SuppressFinalize(this);
    }

    private void OnMainWindowRestoreRequested(object? sender, EventArgs e)
    {
        if (_currentWindow != null)
        {
            RestoreWindow(_currentWindow);
        }
    }
}

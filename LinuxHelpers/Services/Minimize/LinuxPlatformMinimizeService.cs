using Avalonia.Controls;
using LinuxHelpers.Services.ForegroundProgram;
using PlatformAbstractions;
using AVWindow = Avalonia.Controls.Window;

namespace LinuxHelpers.Services.Minimize;

public class LinuxPlatformMinimizeService : IPlatformMinimizeService
{
    private AVWindow? _currentWindow;

    public bool IsSupported => true;

    public bool CanMinimizeToTray => DetectTraySupport();

    public bool CanHideWindow => true;

    public event EventHandler<object>? WindowMinimized;
    public event EventHandler<object>? WindowRestored;
    public event EventHandler<object>? WindowHidden;

    public void SetWindowRef(AVWindow? window)
    {
        _currentWindow = window;
    }

    public void MinimizeWindow(object window)
    {
        if (window is not AVWindow avaloniaWindow) return;
        _currentWindow = avaloniaWindow;

        if (WindowManagerDetector.IsNiri())
        {
            avaloniaWindow.Hide();
            WindowHidden?.Invoke(this, avaloniaWindow);
        }
        else
        {
            avaloniaWindow.WindowState = WindowState.Minimized;
            avaloniaWindow.ShowInTaskbar = false;
            WindowMinimized?.Invoke(this, avaloniaWindow);
        }
    }

    public void RestoreWindow(object window)
    {
        if (window is not AVWindow avaloniaWindow) return;

        avaloniaWindow.ShowInTaskbar = true;
        avaloniaWindow.WindowState = WindowState.Normal;
        avaloniaWindow.Show();
        avaloniaWindow.Activate();
        WindowRestored?.Invoke(this, avaloniaWindow);
    }

    public void HideToBackground(object window)
    {
        if (window is not AVWindow avaloniaWindow) return;
        _currentWindow = avaloniaWindow;
        avaloniaWindow.Hide();
        WindowHidden?.Invoke(this, avaloniaWindow);
    }

    private static bool DetectTraySupport()
    {
        var xdgDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        if (string.IsNullOrEmpty(xdgDesktop)) return false;

        var desktop = xdgDesktop.ToLowerInvariant();
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

    public void Dispose()
    {
        _currentWindow = null;
        GC.SuppressFinalize(this);
    }
}

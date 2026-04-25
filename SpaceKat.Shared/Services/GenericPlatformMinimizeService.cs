using Avalonia.Controls;
using PlatformAbstractions;

namespace SpaceKat.Shared.Services;

/// <summary>
/// 通用平台最小化服务实现
/// 使用基础的Avalonia API，适用于所有平台
/// </summary>
public class GenericPlatformMinimizeService : IPlatformMinimizeService
{
    private Window? _currentWindow;
    private WindowState _previousState;

    public bool IsSupported => true;

    public bool CanMinimizeToTray => false; // 通用实现不支持系统托盘

    public bool CanHideWindow => true;

    public event EventHandler<object>? WindowMinimized;
    public event EventHandler<object>? WindowRestored;
    public event EventHandler<object>? WindowHidden;

    public void SetWindowRef(Window? window)
    {
        _currentWindow = window;
    }

    public void MinimizeWindow(object window)
    {
        if (window is not Window avaloniaWindow) return;
        _currentWindow = avaloniaWindow;
        _previousState = avaloniaWindow.WindowState;

        // 使用基础的窗口状态最小化
        avaloniaWindow.WindowState = WindowState.Minimized;
        WindowMinimized?.Invoke(this, avaloniaWindow);
    }

    public void RestoreWindow(object window)
    {
        if (window is not Window avaloniaWindow) return;
        // 恢复窗口到之前的状态
        avaloniaWindow.WindowState = _previousState == WindowState.Minimized ? WindowState.Normal : _previousState;

        avaloniaWindow.Show();
        avaloniaWindow.WindowState = WindowState.Normal;
        WindowRestored?.Invoke(this, avaloniaWindow);
    }

    public void HideToBackground(object window)
    {
        if (window is not Window avaloniaWindow) return;
        _currentWindow = avaloniaWindow;
        _previousState = avaloniaWindow.WindowState;

        // 隐藏窗口
        avaloniaWindow.Hide();
        WindowHidden?.Invoke(this, avaloniaWindow);
    }

    public void Dispose()
    {
        _currentWindow = null;
        GC.SuppressFinalize(this);
    }
}

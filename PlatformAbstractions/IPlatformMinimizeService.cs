using System;
using Avalonia.Controls;

namespace PlatformAbstractions;

/// <summary>
/// 平台最小化服务接口
/// 提供跨平台的窗口最小化功能支持
/// </summary>
public interface IPlatformMinimizeService : IDisposable
{
    /// <summary>
    /// 当前平台是否支持最小化功能
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// 是否支持最小化到系统托盘
    /// </summary>
    bool CanMinimizeToTray { get; }

    /// <summary>
    /// 是否支持隐藏窗口到后台
    /// </summary>
    bool CanHideWindow { get; }
    
    void SetWindowRef(Window? window);

    /// <summary>
    /// 最小化窗口（根据平台能力选择最佳方式）
    /// </summary>
    /// <param name="window">要最小化的窗口</param>
    void MinimizeWindow(object window);

    /// <summary>
    /// 恢复窗口显示
    /// </summary>
    /// <param name="window">要恢复的窗口</param>
    void RestoreWindow(object window);

    /// <summary>
    /// 隐藏窗口到后台（不使用系统托盘）
    /// </summary>
    /// <param name="window">要隐藏的窗口</param>
    void HideToBackground(object window);

    /// <summary>
    /// 窗口被最小化时触发的事件
    /// </summary>
    event EventHandler<object>? WindowMinimized;

    /// <summary>
    /// 窗口被恢复时触发的事件
    /// </summary>
    event EventHandler<object>? WindowRestored;

    /// <summary>
    /// 窗口被隐藏到后台时触发的事件
    /// </summary>
    event EventHandler<object>? WindowHidden;
}
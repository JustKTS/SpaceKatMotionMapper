using System;

namespace PlatformAbstractions;

/// <summary>
/// 浮动控制窗口服务接口
/// 用于在不支持系统托盘的平台上显示浮动控制窗口
/// </summary>
public interface IFloatingControlWindowService
{
    /// <summary>
    /// 是否支持浮动控制窗口
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// 浮动窗口是否可见
    /// </summary>
    bool IsVisible { get; }

    bool SetMainWindow(object mainWindow);
    /// <summary>
    /// 显示浮动控制窗口
    /// </summary>
    /// <param name="mainWindow">关联的主窗口</param>
    void Show(object mainWindow);

    /// <summary>
    /// 隐藏浮动控制窗口
    /// </summary>
    void Hide();

    /// <summary>
    /// 切换浮动窗口可见性
    /// </summary>
    /// <param name="mainWindow">关联的主窗口</param>
    void Toggle(object mainWindow);

    /// <summary>
    /// 恢复主窗口
    /// </summary>
    void RestoreMainWindow();

    /// <summary>
    /// 退出应用程序
    /// </summary>
    void ExitApplication();

    /// <summary>
    /// 事件：主窗口恢复请求
    /// </summary>
    event EventHandler? MainWindowRestoreRequested;

    /// <summary>
    /// 事件：应用程序退出请求
    /// </summary>
    event EventHandler? ApplicationExitRequested;
}
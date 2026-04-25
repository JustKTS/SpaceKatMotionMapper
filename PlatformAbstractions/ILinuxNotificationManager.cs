namespace PlatformAbstractions;

/// <summary>
/// Linux通知管理器接口
/// 用于防止通知窗口被平铺窗口管理器平铺
/// </summary>
public interface ILinuxNotificationManager
{
    /// <summary>
    /// 是否支持通知防平铺功能
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// 应用防平铺属性到通知窗口
    /// </summary>
    /// <param name="notificationWindow">通知窗口对象</param>
    /// <param name="mainWindow">主窗口对象</param>
    void ApplyAntiTilingProperties(object notificationWindow, object mainWindow);

    /// <summary>
    /// 检查窗口是否为通知类型
    /// </summary>
    /// <param name="window">窗口对象</param>
    /// <returns>是否为通知窗口</returns>
    bool IsNotificationWindow(object window);

    /// <summary>
    /// 检查当前是否运行在平铺窗口管理器上
    /// </summary>
    /// <returns>是否为平铺窗口管理器</returns>
    bool IsRunningOnTilingWindowManager();
}
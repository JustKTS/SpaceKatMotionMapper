using PlatformAbstractions;

namespace LinuxHelpers.Services.ForegroundProgram.Strategies;

/// <summary>
/// 窗口管理器监控策略接口
/// 定义了窗口管理器监控的统一接口，支持多种窗口管理器实现
/// </summary>
public interface IWindowManagerMonitorStrategy : IDisposable
{
    /// <summary>
    /// 前台程序改变事件
    /// 当焦点窗口发生变化时触发
    /// </summary>
    event EventHandler<ForeProgramInfo>? ForeProgramChanged;

    /// <summary>
    /// 是否支持该窗口管理器
    /// 如果不支持，则不会启动监控
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// 开始监控窗口焦点变化
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// 停止监控窗口焦点变化
    /// </summary>
    void StopMonitoring();
}

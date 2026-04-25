using PlatformAbstractions;
using Serilog;
using LinuxHelpers.Services.ForegroundProgram.Strategies;

namespace LinuxHelpers.Services.ForegroundProgram;

/// <summary>
/// Linux 平台前台程序监控服务
/// 根据检测到的窗口管理器类型，使用相应的监控策略
/// </summary>
public class LinuxPlatformForegroundProgramService : IPlatformForegroundProgramService
{
    private readonly IWindowManagerMonitorStrategy? _strategy;
    private bool _disposed;

    /// <summary>
    /// 是否支持前台程序监控
    /// </summary>
    public bool IsSupported => _strategy?.IsSupported ?? false;

    /// <summary>
    /// 前台程序改变事件
    /// </summary>
    public event EventHandler<ForeProgramInfo>? ForeProgramChanged
    {
        add
        {
            if (_strategy != null)
            {
                Log.Debug("[{Service}] Subscribing to ForeProgramChanged event", nameof(LinuxPlatformForegroundProgramService));
                _strategy.ForeProgramChanged += value;
            }
            else
            {
                Log.Warning("[{Service}] Cannot subscribe: no strategy available", nameof(LinuxPlatformForegroundProgramService));
            }
        }
        remove
        {
            if (_strategy != null)
            {
                Log.Debug("[{Service}] Unsubscribing from ForeProgramChanged event", nameof(LinuxPlatformForegroundProgramService));
                _strategy.ForeProgramChanged -= value;
            }
        }
    }

    /// <summary>
    /// 构造函数
    /// 检测窗口管理器并创建相应的监控策略
    /// </summary>
    public LinuxPlatformForegroundProgramService()
    {
        var windowManagerType = WindowManagerDetector.DetectWindowManager();
        Log.Information("[{Service}] Detected window manager: {WindowManager}",
            nameof(LinuxPlatformForegroundProgramService), windowManagerType);

        _strategy = WindowManagerStrategyFactory.CreateStrategy(windowManagerType);

        if (_strategy != null)
        {
            if (_strategy.IsSupported)
            {
                Log.Information("[{Service}] Using strategy: {Strategy}",
                    nameof(LinuxPlatformForegroundProgramService), _strategy.GetType().Name);
                _strategy.StartMonitoring();
            }
            else
            {
                Log.Warning("[{Service}] Detected {WindowManager} but not supported yet",
                    nameof(LinuxPlatformForegroundProgramService), windowManagerType);
            }
        }
        else
        {
            Log.Warning("[{Service}] No strategy available for {WindowManager}",
                nameof(LinuxPlatformForegroundProgramService), windowManagerType);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Log.Debug("[{Service}] Disposing...", nameof(LinuxPlatformForegroundProgramService));
        _strategy?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

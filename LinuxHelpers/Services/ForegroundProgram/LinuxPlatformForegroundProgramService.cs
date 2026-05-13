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
    private static readonly ILogger Log = Serilog.Log.ForContext<LinuxPlatformForegroundProgramService>();

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
                Log.Debug("Subscribing to ForeProgramChanged event");
                _strategy.ForeProgramChanged += value;
            }
            else
            {
                Log.Warning("Cannot subscribe: no strategy available");
            }
        }
        remove
        {
            if (_strategy != null)
            {
                Log.Debug("Unsubscribing from ForeProgramChanged event");
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
        Log.Information("Detected window manager: {WindowManager}",
            windowManagerType);

        _strategy = WindowManagerStrategyFactory.CreateStrategy(windowManagerType);

        if (_strategy != null)
        {
            if (_strategy.IsSupported)
            {
                Log.Information("Using strategy: {Strategy}",
                    _strategy.GetType().Name);
                _strategy.StartMonitoring();
            }
            else
            {
                Log.Warning("Detected {WindowManager} but not supported yet",
                    windowManagerType);
            }
        }
        else
        {
            Log.Warning("No strategy available for {WindowManager}",
                windowManagerType);
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

        Log.Debug("Disposing...");
        _strategy?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

using LinuxHelpers.Services.ForegroundProgram.Strategies;

namespace LinuxHelpers.Services.ForegroundProgram;

/// <summary>
/// 窗口管理器监控策略工厂
/// 根据窗口管理器类型创建对应的监控策略
/// </summary>
public static class WindowManagerStrategyFactory
{
    /// <summary>
    /// 创建窗口管理器监控策略
    /// </summary>
    /// <param name="type">窗口管理器类型</param>
    /// <returns>对应的监控策略实例，如果不支持则返回 null</returns>
    public static IWindowManagerMonitorStrategy? CreateStrategy(WindowManagerType type)
    {
        return type switch
        {
            WindowManagerType.Niri => new NiriWindowManagerStrategy(),
            WindowManagerType.Hyprland => new HyprlandWindowManagerStrategy(),
            WindowManagerType.Kde => new KdeWindowManagerStrategy(),
            _ => null
        };
    }
}

using LinuxHelpers.Services.ForegroundProgram.Strategies;

namespace LinuxHelpers.Services.ForegroundProgram;

public static class WindowManagerStrategyFactory
{
    public static IWindowManagerMonitorStrategy? CreateStrategy(WindowManagerType type)
    {
        return type switch
        {
            WindowManagerType.Niri => new NiriWindowManagerStrategy(),
            _ => null
        };
    }
}

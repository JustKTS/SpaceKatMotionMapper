using PlatformAbstractions;
using Serilog;

namespace LinuxHelpers.Services.ForegroundProgram.Strategies;

/// <summary>
/// Hyprland 窗口管理器监控策略
/// TODO: 尚未实现，欢迎贡献 PR！
///
/// 实现参考：
/// - Hyprland IPC 文档: https://wiki.hyprland.org/IPC/
/// - 使用 hyprctl 或者 socket IPC 监听事件
/// - 监听 activewindowv2 或类似事件
/// </summary>
[Obsolete("Hyprland support is not yet implemented. PRs welcome!")]
public class HyprlandWindowManagerStrategy : IWindowManagerMonitorStrategy
{
    public bool IsSupported => false;

    public event EventHandler<ForeProgramInfo>? ForeProgramChanged
    {
        add
        {
            Log.Warning("[{Strategy}] Hyprland monitoring is not yet implemented. PRs welcome! [{value}]",
                nameof(HyprlandWindowManagerStrategy), value);
        }
        remove
        {
        }
    }

    public void StartMonitoring()
    {
        Log.Warning("[{Strategy}] Hyprland monitoring is not yet implemented. PRs welcome!", nameof(HyprlandWindowManagerStrategy));
    }

    public void StopMonitoring()
    {
        // 空实现
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

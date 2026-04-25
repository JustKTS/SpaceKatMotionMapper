using PlatformAbstractions;
using Serilog;

namespace LinuxHelpers.Services.ForegroundProgram.Strategies;

/// <summary>
/// KDE (KWin) 窗口管理器监控策略
/// TODO: 尚未实现，欢迎贡献 PR！
///
/// 实现参考：
/// - KDE D-Bus API: https://develop.kde.org/docs/platform/scripting/
/// - 使用 D-Bus 接口监听窗口变化
/// - 监听 org.kde.KWin /KWin 当前活动窗口变化
/// - 或者使用 qdbus 命令行工具
/// </summary>
[Obsolete("KDE support is not yet implemented. PRs welcome!")]
public class KdeWindowManagerStrategy : IWindowManagerMonitorStrategy
{
    public bool IsSupported => false;

    public event EventHandler<ForeProgramInfo>? ForeProgramChanged
    {
        add
        {
            Log.Warning("[{Strategy}] KDE monitoring is not yet implemented. PRs welcome! [{value}]", nameof(KdeWindowManagerStrategy), value);
        }
        remove
        {
        }
    }

    public void StartMonitoring()
    {
        Log.Warning("[{Strategy}] KDE monitoring is not yet implemented. PRs welcome!", nameof(KdeWindowManagerStrategy));
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

namespace PlatformAbstractions;

/// <summary>
/// 平台热键服务接口
/// </summary>
public interface IPlatformHotKeyService
{
    /// <summary>
    /// 注册热键
    /// </summary>
    /// <param name="handle">窗口句柄</param>
    /// <param name="id">热键ID</param>
    /// <param name="fsModifiers">修饰键</param>
    /// <param name="vk">虚拟键码</param>
    /// <returns>是否注册成功</returns>
    bool RegisterHotKeyWrapper(nint handle, int id, int fsModifiers, int vk);

    /// <summary>
    /// 注销热键
    /// </summary>
    /// <param name="handle">窗口句柄</param>
    /// <param name="id">热键ID</param>
    /// <returns>是否注销成功</returns>
    bool UnregisterHotKeyWrapper(nint handle, int id);

    /// <summary>
    /// 检查平台是否支持热键功能
    /// </summary>
    bool IsSupported { get; }
}
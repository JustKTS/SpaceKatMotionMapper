using System;

namespace PlatformAbstractions;

/// <summary>
/// 平台前台程序监控服务接口
/// </summary>
public interface IPlatformForegroundProgramService : IDisposable
{
    /// <summary>
    /// 前台程序改变事件
    /// </summary>
    event EventHandler<ForeProgramInfo>? ForeProgramChanged;

    /// <summary>
    /// 检查平台是否支持前台程序监控
    /// </summary>
    bool IsSupported { get; }
}
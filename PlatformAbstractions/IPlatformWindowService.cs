using System.Collections.Generic;
using System.Threading;

namespace PlatformAbstractions;

/// <summary>
/// 平台窗口信息服务接口
/// </summary>
public interface IPlatformWindowService
{
    /// <summary>
    /// 获取所有前台程序信息
    /// </summary>
    /// <returns>前台程序信息列表</returns>
    IReadOnlyList<ForeProgramInfo> FindAllForegroundPrograms();

    /// <summary>
    /// 异步枚举所有前台程序信息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>前台程序信息异步枚举</returns>
    IAsyncEnumerable<ForeProgramInfo> FindAllForegroundProgramsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 前台程序信息
/// </summary>
public record ForeProgramInfo(string Title, string ProcessName, string ClassName, string ProcessFileAddress)
{
    public ForeProgramInfo() : this(Title: string.Empty, ProcessName: string.Empty, ClassName: string.Empty,
        ProcessFileAddress: string.Empty)
    {
    }
}
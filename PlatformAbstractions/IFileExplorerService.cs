namespace PlatformAbstractions;

/// <summary>
/// 平台文件/文件夹打开服务接口
/// </summary>
public interface IFileExplorerService
{
    /// <summary>
    /// 打开指定路径的文件夹或文件
    /// </summary>
    /// <param name="path">要打开的文件夹或文件路径</param>
    void OpenPath(string path);
}

using System;
using System.IO;
using PlatformAbstractions;
using SpaceKat.Shared.States;

namespace LinuxHelpers.Services.SingletonInstance;

/// <summary>
/// Linux 平台单例服务，基于文件锁实现。
/// 使用 FileStream + FileShare.None 确保跨会话的互斥性，
/// 避免 .NET 命名 Mutex 在 Linux 上的会话隔离问题。
/// </summary>
public class LinuxSingletonInstanceService : ISingletonInstanceService
{
    private FileStream? _lockStream;

    public bool TryAcquire()
    {
        var lockDir = GlobalPaths.AppDataPath;
        Directory.CreateDirectory(lockDir);

        var lockPath = Path.Combine(lockDir, ".instance.lock");

        try
        {
            _lockStream = new FileStream(
                lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_lockStream != null)
        {
            try
            {
                _lockStream.Dispose();
            }
            catch
            {
                // ignore
            }
        }
    }
}

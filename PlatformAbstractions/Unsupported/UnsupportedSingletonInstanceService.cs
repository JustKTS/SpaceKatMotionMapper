using System;
using System.Threading;

namespace PlatformAbstractions.Unsupported;

/// <summary>
/// 非 Windows/Linux 平台的单例服务回退实现，基于命名 Mutex。
/// </summary>
public class UnsupportedSingletonInstanceService : ISingletonInstanceService
{
    private readonly Mutex _mutex;
    private readonly bool _acquired;

    public UnsupportedSingletonInstanceService()
    {
        _mutex = new Mutex(true, "SpaceMotionMapper", out _acquired);
    }

    public bool TryAcquire() => _acquired;

    public void Dispose()
    {
        if (_acquired)
        {
            _mutex.ReleaseMutex();
        }
        _mutex.Dispose();
    }
}

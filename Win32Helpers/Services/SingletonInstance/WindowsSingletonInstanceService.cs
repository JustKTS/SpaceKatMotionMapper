#if WINDOWS
using System;
using System.Threading;
using PlatformAbstractions;

namespace Win32Helpers.Services.SingletonInstance;

/// <summary>
/// Windows 平台单例服务，基于命名 Mutex 实现。
/// </summary>
public class WindowsSingletonInstanceService : ISingletonInstanceService
{
    private readonly Mutex _mutex;
    private readonly bool _acquired;

    public WindowsSingletonInstanceService()
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
#endif

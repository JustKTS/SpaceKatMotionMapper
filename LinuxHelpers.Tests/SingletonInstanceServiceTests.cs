#if LINUX
using PlatformAbstractions;
using LinuxHelpers.Services.SingletonInstance;

namespace LinuxHelpers.Tests;

[NotInParallel]
public class SingletonInstanceServiceTests
{
    [After(Test)]
    public void Cleanup()
    {
        var lockPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SpaceKatMotionMapper",
            ".instance.lock");
        if (File.Exists(lockPath))
        {
            try { File.Delete(lockPath); } catch { /* ignore */ }
        }
    }

    [Test]
    public async Task TryAcquire_FirstCall_ReturnsTrue()
    {
        using var sut = new LinuxSingletonInstanceService();

        await Assert.That(sut.TryAcquire()).IsTrue();
    }

    [Test]
    public async Task TryAcquire_AfterAcquire_ReturnsFalse()
    {
        using var first = new LinuxSingletonInstanceService();
        var acquiredFirst = first.TryAcquire();

        using var second = new LinuxSingletonInstanceService();
        var acquiredSecond = second.TryAcquire();

        await Assert.That(acquiredFirst).IsTrue();
        await Assert.That(acquiredSecond).IsFalse();
    }

    [Test]
    public async Task Dispose_ReleasesLock_AllowsNewAcquisition()
    {
        var first = new LinuxSingletonInstanceService();
        first.TryAcquire();
        first.Dispose();

        using var second = new LinuxSingletonInstanceService();

        await Assert.That(second.TryAcquire()).IsTrue();
    }

    [Test]
    public async Task Implements_ISingletonInstanceService()
    {
        ISingletonInstanceService sut = new LinuxSingletonInstanceService();

        await Assert.That(sut).IsNotNull();
    }
}
#else
namespace LinuxHelpers.Tests;

public class SingletonInstanceServiceTests
{
    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task TryAcquire_FirstCall_ReturnsTrue() { }

    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task TryAcquire_AfterAcquire_ReturnsFalse() { }

    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task Dispose_ReleasesLock_AllowsNewAcquisition() { }

    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task Implements_ISingletonInstanceService() { }
}
#endif

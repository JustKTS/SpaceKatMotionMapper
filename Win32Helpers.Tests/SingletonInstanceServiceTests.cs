#if WINDOWS
using PlatformAbstractions;
using Win32Helpers.Services.SingletonInstance;

namespace Win32Helpers.Tests;

[NotInParallel]
public class SingletonInstanceServiceTests
{
    [Test]
    public async Task TryAcquire_FirstCall_ReturnsTrue()
    {
        using var sut = new WindowsSingletonInstanceService();

        await Assert.That(sut.TryAcquire()).IsTrue();
    }

    [Test]
    public async Task TryAcquire_SecondInstance_ReturnsFalse()
    {
        using var first = new WindowsSingletonInstanceService();
        var acquiredFirst = first.TryAcquire();

        using var second = new WindowsSingletonInstanceService();
        var acquiredSecond = second.TryAcquire();

        await Assert.That(acquiredFirst).IsTrue();
        await Assert.That(acquiredSecond).IsFalse();
    }

    [Test]
    public async Task Dispose_ReleasesMutex_AllowsNewAcquisition()
    {
        var first = new WindowsSingletonInstanceService();
        first.TryAcquire();
        first.Dispose();

        using var second = new WindowsSingletonInstanceService();

        await Assert.That(second.TryAcquire()).IsTrue();
    }

    [Test]
    public async Task Implements_ISingletonInstanceService()
    {
        ISingletonInstanceService sut = new WindowsSingletonInstanceService();

        await Assert.That(sut).IsNotNull();
    }
}
#else
namespace Win32Helpers.Tests;

public class SingletonInstanceServiceTests
{
    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task TryAcquire_FirstCall_ReturnsTrue() { }

    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task TryAcquire_SecondInstance_ReturnsFalse() { }

    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task Dispose_ReleasesMutex_AllowsNewAcquisition() { }

    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task Implements_ISingletonInstanceService() { }
}
#endif

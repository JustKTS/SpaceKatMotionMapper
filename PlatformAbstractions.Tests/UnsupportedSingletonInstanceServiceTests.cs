using PlatformAbstractions;
using PlatformAbstractions.Unsupported;

namespace PlatformAbstractions.Tests;

[NotInParallel]
public class UnsupportedSingletonInstanceServiceTests
{
    [Test]
    public async Task TryAcquire_FirstCall_ReturnsTrue()
    {
        using var sut = new UnsupportedSingletonInstanceService();

        await Assert.That(sut.TryAcquire()).IsTrue();
    }

    [Test]
    public async Task TryAcquire_SecondInstance_ReturnsFalse()
    {
        using var first = new UnsupportedSingletonInstanceService();
        var acquiredFirst = first.TryAcquire();

        using var second = new UnsupportedSingletonInstanceService();
        var acquiredSecond = second.TryAcquire();

        await Assert.That(acquiredFirst).IsTrue();
        await Assert.That(acquiredSecond).IsFalse();
    }

    [Test]
    public async Task Dispose_ReleasesMutex_AllowsNewAcquisition()
    {
        var first = new UnsupportedSingletonInstanceService();
        first.TryAcquire();
        first.Dispose();

        using var second = new UnsupportedSingletonInstanceService();

        await Assert.That(second.TryAcquire()).IsTrue();
    }

    [Test]
    public async Task Implements_ISingletonInstanceService()
    {
        ISingletonInstanceService sut = new UnsupportedSingletonInstanceService();

        await Assert.That(sut).IsNotNull();
    }
}

using PlatformAbstractions.Unsupported;

namespace PlatformAbstractions.Tests;

public class PlatformTests
{
    [Test]
    public async Task GetPlatformUnsupportedMessage_ShouldContainFeatureNameAndWindowsHint()
    {
        var message = PlatformHelper.GetPlatformUnsupportedMessage("热键注册");

        await Assert.That(message.Contains("热键注册")).IsTrue();
        await Assert.That(message.Contains("Windows")).IsTrue();
    }

    [Test]
    public async Task UnsupportedPlatformHotKeyService_ShouldAlwaysReturnUnsupported()
    {
        var sut = new UnsupportedPlatformHotKeyService();

        await Assert.That(sut.IsSupported).IsFalse();
        await Assert.That(sut.RegisterHotKeyWrapper(0, 1, 0, 65)).IsFalse();
        await Assert.That(sut.UnregisterHotKeyWrapper(0, 1)).IsFalse();
    }

    [Test]
    public async Task UnsupportedPlatformWindowService_ShouldReturnNoForegroundPrograms()
    {
        var sut = new UnsupportedPlatformWindowService();

        var programs = sut.FindAllForegroundPrograms();
        await Assert.That(programs.Count).IsEqualTo(0);
    }
}

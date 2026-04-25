using PlatformAbstractions.Unsupported;

namespace PlatformAbstractions.Tests;

public class FileExplorerServiceTests
{
    [Test]
    public async Task UnsupportedFileExplorerService_OpenPath_ShouldNotThrow()
    {
        var sut = new UnsupportedFileExplorerService();

        await Assert.That(async () => sut.OpenPath("/test/path")).ThrowsNothing();
    }

    [Test]
    public async Task UnsupportedFileExplorerService_OpenPath_WithNullPath_ShouldNotThrow()
    {
        var sut = new UnsupportedFileExplorerService();

        await Assert.That(async () => sut.OpenPath(null!)).ThrowsNothing();
    }

    [Test]
    public async Task UnsupportedFileExplorerService_ShouldImplementIFileExplorerService()
    {
        IFileExplorerService service = new UnsupportedFileExplorerService();

        await Assert.That(service).IsNotNull();
    }
}

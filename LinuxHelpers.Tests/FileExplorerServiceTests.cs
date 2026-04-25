#if LINUX
using PlatformAbstractions;
using LinuxHelpers.Services.FileExplorer;

namespace LinuxHelpers.Tests;

public class FileExplorerServiceTests
{
    [Test]
    public async Task LinuxFileExplorerService_ShouldUseXdgOpen()
    {
        var sut = new LinuxFileExplorerService();

        await Assert.That(sut.ExecutableName).IsEqualTo("xdg-open");
    }

    [Test]
    public async Task LinuxFileExplorerService_ShouldPassPathToProcess()
    {
        string? capturedExe = null;
        string? capturedArgs = null;

        var sut = new TestableLinuxFileExplorerService((exe, args) =>
        {
            capturedExe = exe;
            capturedArgs = args;
        });

        sut.OpenPath("/home/user/config");

        await Assert.That(capturedExe).IsEqualTo("xdg-open");
        await Assert.That(capturedArgs).IsEqualTo("/home/user/config");
    }

    [Test]
    public async Task LinuxFileExplorerService_ShouldImplementIFileExplorerService()
    {
        IFileExplorerService service = new LinuxFileExplorerService();

        await Assert.That(service).IsNotNull();
    }

    private class TestableLinuxFileExplorerService(Action<string, string> onStartProcess)
        : LinuxFileExplorerService
    {
        protected override void OnStartProcess(string executable, string arguments) =>
            onStartProcess(executable, arguments);
    }
}
#else
namespace LinuxHelpers.Tests;

public class FileExplorerServiceTests
{
    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task LinuxFileExplorerService_ShouldUseXdgOpen() { }

    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task LinuxFileExplorerService_ShouldPassPathToProcess() { }

    [Test]
    [Skip("Linux 专用测试，当前平台不支持")]
    public async Task LinuxFileExplorerService_ShouldImplementIFileExplorerService() { }
}
#endif

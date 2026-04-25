#if WINDOWS
using PlatformAbstractions;
using Win32Helpers.Windows;

namespace Win32Helpers.Tests;

public class FileExplorerServiceTests
{
    [Test]
    public async Task WindowsFileExplorerService_ShouldUseExplorerExe()
    {
        var sut = new WindowsFileExplorerService();

        await Assert.That(sut.ExecutableName).IsEqualTo("explorer.exe");
    }

    [Test]
    public async Task WindowsFileExplorerService_ShouldPassPathToProcess()
    {
        string? capturedExe = null;
        string? capturedArgs = null;

        var sut = new TestableWindowsFileExplorerService((exe, args) =>
        {
            capturedExe = exe;
            capturedArgs = args;
        });

        sut.OpenPath(@"C:\TestPath");

        await Assert.That(capturedExe).IsEqualTo("explorer.exe");
        await Assert.That(capturedArgs).IsEqualTo(@"C:\TestPath");
    }

    [Test]
    public async Task WindowsFileExplorerService_ShouldImplementIFileExplorerService()
    {
        IFileExplorerService service = new WindowsFileExplorerService();

        await Assert.That(service).IsNotNull();
    }

    private class TestableWindowsFileExplorerService(Action<string, string> onStartProcess)
        : WindowsFileExplorerService
    {
        protected override void OnStartProcess(string executable, string arguments) =>
            onStartProcess(executable, arguments);
    }
}
#else
namespace Win32Helpers.Tests;

public class FileExplorerServiceTests
{
    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task WindowsFileExplorerService_ShouldUseExplorerExe() { }

    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task WindowsFileExplorerService_ShouldPassPathToProcess() { }

    [Test]
    [Skip("Windows 专用测试，当前平台不支持")]
    public async Task WindowsFileExplorerService_ShouldImplementIFileExplorerService() { }
}
#endif
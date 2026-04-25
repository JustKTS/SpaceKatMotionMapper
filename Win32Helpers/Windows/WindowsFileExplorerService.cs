using System.Diagnostics;
using PlatformAbstractions;

namespace Win32Helpers.Windows;

public class WindowsFileExplorerService : IFileExplorerService
{
    public virtual string ExecutableName => "explorer.exe";

    public void OpenPath(string path) => OnStartProcess(ExecutableName, path);

    protected virtual void OnStartProcess(string executable, string arguments) =>
        Process.Start(executable, arguments);
}

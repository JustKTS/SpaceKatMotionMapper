using System.Diagnostics;
using PlatformAbstractions;

namespace LinuxHelpers.Services.FileExplorer;

public class LinuxFileExplorerService : IFileExplorerService
{
    public virtual string ExecutableName => "xdg-open";

    public void OpenPath(string path) => OnStartProcess(ExecutableName, path);

    protected virtual void OnStartProcess(string executable, string arguments) =>
        Process.Start(executable, arguments);
}

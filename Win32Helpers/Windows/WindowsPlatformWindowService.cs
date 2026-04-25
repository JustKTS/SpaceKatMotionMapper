#if WINDOWS
using PlatformAbstractions;
using System.Diagnostics.CodeAnalysis;

namespace Win32Helpers.Windows;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public class WindowsPlatformWindowService : IPlatformWindowService
{
    public IReadOnlyList<ForeProgramInfo> FindAllForegroundPrograms()
    {
        return CurrentForeProgramHelper.FindAll();
    }

    public IAsyncEnumerable<ForeProgramInfo> FindAllForegroundProgramsAsync(CancellationToken cancellationToken = default)
    {
        return CurrentForeProgramHelper.FindAllAsyncEnumerable(cancellationToken);
    }
}
#endif
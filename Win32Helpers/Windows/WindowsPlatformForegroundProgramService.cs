#if WINDOWS
using PlatformAbstractions;
using System.Diagnostics.CodeAnalysis;

namespace Win32Helpers.Windows;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public class WindowsPlatformForegroundProgramService : IPlatformForegroundProgramService
{
    private readonly CurrentForeProgramHelper _helper;

    public WindowsPlatformForegroundProgramService()
    {
        _helper = new CurrentForeProgramHelper();
    }

    public bool IsSupported => true;


    public event EventHandler<ForeProgramInfo>? ForeProgramChanged
    {
        add => _helper.ForeProgramChanged += value;
        remove => _helper.ForeProgramChanged -= value;
    }

    public void Dispose()
    {
        _helper.Dispose();
        GC.SuppressFinalize(this);
    }
}
#endif
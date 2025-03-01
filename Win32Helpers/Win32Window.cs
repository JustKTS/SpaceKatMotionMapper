using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Windows.Win32.Foundation;

using static Windows.Win32.PInvoke;
// ReSharper disable InconsistentNaming

namespace Win32Helpers;
[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public class Win32Window
{
    private readonly HWND _hWnd;
    private string? _className;
    private string? _title;
    private string? _processName;
    private uint _pid;
    private string? _processFileAddress;

    internal Win32Window(nint handle)
    {
        _hWnd = (HWND)handle;
    }

    // public nint Handle => _hWnd;

    public string ClassName => _className ??= Helpers.CallWin32ToGetPWSTR(512, (p, l) => GetClassName(_hWnd, p, l));

    public string Title => _title ??=  Helpers.CallWin32ToGetPWSTR(512, (p, l) => GetWindowText(_hWnd, p, l));

    public uint ProcessId => _pid is 0 ? (_pid = GetProcessIdCore()) : _pid;

    public string ProcessName => _processName ??= Process.GetProcessById((int)ProcessId).ProcessName;
    
    public string ProcessFileAddress =>  _processFileAddress??= GetExecutablePathFromProcessId((int)ProcessId);

    private unsafe uint GetProcessIdCore()
    {
        uint pid = 0;
        _ = GetWindowThreadProcessId(_hWnd, &pid);
        return pid;
    }

    private static string GetExecutablePathFromProcessId(int processId)
    {
        if (processId == 0)
        {
           return string.Empty;
        }

        var p_correct = Process.GetProcesses().FirstOrDefault(p => p.Id.Equals(processId));
        try
        {
            return p_correct?.MainModule is null ? string.Empty : p_correct.MainModule.FileName;

        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

}
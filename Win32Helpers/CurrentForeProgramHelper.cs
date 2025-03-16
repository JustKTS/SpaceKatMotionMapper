using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using static Windows.Win32.PInvoke;

namespace Win32Helpers;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public class CurrentForeProgramHelper : IDisposable
{
    public event EventHandler<ForeProgramInfo>? ForeProgramChanged;

    private GCHandle _gcHandle;
    private readonly HWINEVENTHOOK _hWinEventHook;

    public CurrentForeProgramHelper()
    {
        var winEventProc = new WINEVENTPROC(WinEventProcFunc);
        _gcHandle = GCHandle.Alloc(winEventProc, GCHandleType.Normal);
        // 监听系统的前台窗口变化。
        _hWinEventHook = SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
            HMODULE.Null, winEventProc,
            0, 0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
        // 开启消息循环，以便 WinEventProc 能够被调用。
        if (!GetMessage(out var lpMsg, default, 0, 0)) return;
        TranslateMessage(in lpMsg);
        DispatchMessage(in lpMsg);
    }


    // 当前前台窗口变化时，输出新的前台窗口信息。
    private void WinEventProcFunc(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd, int idObject, int idChild,
        uint idEventThread, uint dwmsEventTime)
    {
        try
        {
            var current = GetForegroundWindow();
            // 随后获取窗口标题、类名等……
            var w = new Win32Window(current);
            ForeProgramChanged?.Invoke(null,
                new ForeProgramInfo(w.Title, w.ProcessName, w.ClassName, w.ProcessFileAddress));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        UnhookWinEvent(_hWinEventHook);
        if (_gcHandle.IsAllocated)
        {
            _gcHandle.Free();
        }

        GC.SuppressFinalize(this);
    }

    public static IReadOnlyList<ForeProgramInfo> FindAll()
    {
        var processNameSet = new HashSet<string>();
        var windowList = new List<ForeProgramInfo>();
        EnumWindows(OnWindowEnum, 0);

        return windowList;

        BOOL OnWindowEnum(HWND hWnd, LPARAM param1)
        {
            if (!GetParent(hWnd).IsNull) return true;
            if (!IsWindowVisible(hWnd)) return true;
            var w = new Win32Window(hWnd);
            if (!processNameSet.Add(w.ProcessName)) return true;
            windowList.Add(new ForeProgramInfo(w.Title, w.ProcessName, w.ClassName, w.ProcessFileAddress));
            return true;
        }
    }
}

public record ForeProgramInfo(string Title, string ProcessName, string ClassName, string ProcessFileAddress)
{
    public ForeProgramInfo() : this(Title: string.Empty, ProcessName: string.Empty, ClassName: string.Empty,
        ProcessFileAddress: string.Empty)
    {
    }
}
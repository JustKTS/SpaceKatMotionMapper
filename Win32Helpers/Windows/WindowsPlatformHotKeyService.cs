#if WINDOWS
using PlatformAbstractions;
using System.Runtime.InteropServices;

namespace Win32Helpers.Windows;

public partial class WindowsPlatformHotKeyService : IPlatformHotKeyService
{
    public bool IsSupported => true;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public bool RegisterHotKeyWrapper(IntPtr handle, int id, int fsModifiers, int vk)
    {
        return RegisterHotKey(handle, id, fsModifiers, vk);
    }

    public bool UnregisterHotKeyWrapper(IntPtr handle, int id)
    {
        return UnregisterHotKey(handle, id);
    }
}
#endif
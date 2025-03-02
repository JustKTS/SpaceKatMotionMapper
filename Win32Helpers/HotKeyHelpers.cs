using System.Runtime.InteropServices;

namespace Win32Helpers;

public static partial class HotKeyHelpers
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public static bool RegisterHotKeyWrapper(IntPtr hWnd, int id, int fsModifiers, int vk)
    {
        return RegisterHotKey(hWnd, id, fsModifiers, vk);
    }

    public static bool UnregisterHotKeyWrapper(IntPtr hWnd, int id)
    {
        return UnregisterHotKey(hWnd, id);
    }
}
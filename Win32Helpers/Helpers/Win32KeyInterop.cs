#if WINDOWS
using Avalonia.Input;
using Avalonia.Win32.Input;
using SpaceKat.Shared.Models;

namespace Win32Helpers.Helpers;

/// <summary>
/// Windows-only: KeyCodeWrapper 与 Avalonia Key 的互转（依赖 Avalonia.Win32.Input.KeyInterop）
/// </summary>
public static class Win32KeyInterop
{
    public static Key ToAvaloniaKey(this KeyCodeWrapper keyCode)
    {
        return KeyInterop.KeyFromVirtualKey((int)keyCode, 0);
    }

    public static KeyCodeWrapper ToKeyCodeWrapper(this Key key)
    {
        return (KeyCodeWrapper)KeyInterop.VirtualKeyFromKey(key);
    }
}
#endif

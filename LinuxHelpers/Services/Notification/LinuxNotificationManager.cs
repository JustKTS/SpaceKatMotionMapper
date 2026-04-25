using System.Runtime.InteropServices;
using Avalonia.Controls;
using PlatformAbstractions;
using AVWindow = Avalonia.Controls.Window;

namespace LinuxHelpers.Services.Notification;

/// <summary>
/// Linux通知管理器实现
/// 用于防止通知窗口被平铺窗口管理器平铺
/// </summary>
public class LinuxNotificationManager : ILinuxNotificationManager
{
    // TODO: AI代码，需要验证或精简
    private bool? _isTilingWindowManager;
    private readonly string[] _tilingWindowManagers = new[]
    {
        "i3", "sway", "bspwm", "dwm", "awesome", "xmonad", "qtile",
        "herbstluftwm", "niri", "hyprland", "river", "wayfire"
    };

    public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// 检查当前是否运行在平铺窗口管理器上
    /// </summary>
    public bool IsRunningOnTilingWindowManager()
    {
        if (_isTilingWindowManager.HasValue)
            return _isTilingWindowManager.Value;

        try
        {
            // 检查XDG_CURRENT_DESKTOP环境变量
            var xdgDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
            if (!string.IsNullOrEmpty(xdgDesktop))
            {
                var desktop = xdgDesktop.ToLowerInvariant();
                if (_tilingWindowManagers.Any(desktop.Contains))
                {
                    _isTilingWindowManager = true;
                    return true;
                }
            }

            // 检查XDG_SESSION_DESKTOP
            var sessionDesktop = Environment.GetEnvironmentVariable("XDG_SESSION_DESKTOP");
            if (!string.IsNullOrEmpty(sessionDesktop))
            {
                var desktop = sessionDesktop.ToLowerInvariant();
                if (_tilingWindowManagers.Any(desktop.Contains))
                {
                    _isTilingWindowManager = true;
                    return true;
                }
            }

            // 对于Wayland，假设可能是平铺WM（更保守的方法）
            var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            if (!string.IsNullOrEmpty(waylandDisplay))
            {
                // 在Wayland环境下，许多窗口管理器都是平铺的
                // 但也有像GNOME这样的传统窗口管理器
                // 这里我们采用保守策略：只有在明确检测到平铺WM时才返回true
                _isTilingWindowManager = false;
                return false;
            }

            // 默认情况下假设不是平铺窗口管理器
            _isTilingWindowManager = false;
            return false;
        }
        catch
        {
            _isTilingWindowManager = false;
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否为通知类型
    /// </summary>
    public bool IsNotificationWindow(object window)
    {
        if (window is not AVWindow avaloniaWindow) return false;
        // 检查窗口标题或类名
        var title = avaloniaWindow.Title?.ToLowerInvariant() ?? "";

        // 检查是否包含通知相关的关键字
        if (title.Contains("notification") ||
            title.Contains("提示") ||
            title.Contains("通知") ||
            title.Contains("alert"))
        {
            return true;
        }

        // 检查窗口类名（如果可用）
        var className = avaloniaWindow.GetType().Name.ToLowerInvariant();
        if (className.Contains("notification") ||
            className.Contains("popup") ||
            className.Contains("toast"))
        {
            return true;
        }

        // 检查窗口属性
        if (avaloniaWindow is { ShowInTaskbar: false, Topmost: true })
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 应用防平铺属性到通知窗口
    /// </summary>
    public void ApplyAntiTilingProperties(object notificationWindow, object mainWindow)
    {
        if (!IsRunningOnTilingWindowManager())
            return;

        if (notificationWindow is AVWindow avaloniaNotification &&
            mainWindow is AVWindow avaloniaMain)
        {
            try
            {
                // 设置窗口属性以防止被平铺
                avaloniaNotification.ShowInTaskbar = false;

                // 检查是否为Wayland环境
                if (IsWaylandEnvironment())
                {
                    // Wayland环境的处理方式
                    ApplyWaylandAntiTilingProperties(avaloniaNotification, avaloniaMain);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // X11环境的处理方式
                    ApplyX11AntiTilingProperties(avaloniaNotification, avaloniaMain);
                }
            }
            catch
            {
                // 如果设置失败，窗口仍然可以正常工作
            }
        }
    }

    /// <summary>
    /// 检查是否为Wayland环境
    /// </summary>
    private static bool IsWaylandEnvironment()
    {
        var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        var xdgSessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");

        return !string.IsNullOrEmpty(waylandDisplay) ||
               (xdgSessionType?.Equals("wayland", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// 应用Wayland环境的防平铺属性
    /// </summary>
    private static void ApplyWaylandAntiTilingProperties(AVWindow notificationWindow, AVWindow mainWindow)
    {
        // Wayland环境下的窗口管理需要通过协议层设置
        // 目前Avalonia对Wayland的支持有限，这里主要设置窗口属性

        try
        {
            // 设置窗口基本属性
            notificationWindow.ShowInTaskbar = true;
            notificationWindow.Topmost = true;

            // 尝试设置窗口类型（如果Avalonia支持）
            // Wayland环境下，窗口类型通过wayland协议设置
            // 这里可能需要使用特定的wayland协议扩展
        }
        catch
        {
            // Wayland设置失败，窗口仍然可以正常显示
        }
    }

    /// <summary>
    /// 应用X11特定的防平铺属性
    /// </summary>
    private void ApplyX11AntiTilingProperties(AVWindow notificationWindow, AVWindow mainWindow)
    {
        try
        {
            // 设置窗口类型为NOTIFICATION
            SetX11WindowType(notificationWindow, "_NET_WM_WINDOW_TYPE_NOTIFICATION");

            // 设置窗口状态
            SetX11WindowState(notificationWindow, "_NET_WM_STATE_ABOVE");

            // 设置 transient for，让通知窗口关联到主窗口
            SetTransientFor(notificationWindow, mainWindow);

            // 设置跳过任务栏和分页器
            SetX11WindowState(notificationWindow, "_NET_WM_STATE_SKIP_TASKBAR");
            SetX11WindowState(notificationWindow, "_NET_WM_STATE_SKIP_PAGER");
        }
        catch
        {
            // X11调用失败，但窗口仍然可以正常工作
        }
    }

    private void SetX11WindowType(AVWindow window, string windowType)
    {
        try
        {
            var display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
                return;

            var atom = XInternAtom(display, windowType, false);
            if (atom != 0)
            {
                var windowId = GetX11WindowId(window);
                if (windowId != 0)
                {
                    var atomLong = (long)atom;
                    XChangeProperty(
                        display,
                        windowId,
                        XInternAtom(display, "_NET_WM_WINDOW_TYPE", false),
                        XaAtom,
                        32,
                        PropModeReplace,
                        ref atomLong,
                        1
                    );
                }
            }

            XCloseDisplay(display);
        }
        catch
        {
            // 忽略X11调用失败
        }
    }

    private void SetX11WindowState(AVWindow window, string state)
    {
        try
        {
            var display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
                return;

            var atom = XInternAtom(display, state, false);
            if (atom != 0)
            {
                var windowId = GetX11WindowId(window);
                if (windowId != 0)
                {
                    var rootWindow = DefaultRootWindow(display);

                    var stateAtom = XInternAtom(display, "_NET_WM_STATE", false);
                    var clientMessage = new XClientMessageEvent
                    {
                        type = ClientMessage,
                        serial = 0,
                        send_event = true,
                        display = display,
                        window = windowId,
                        message_type = (long)stateAtom,
                        format = 32,
                        data = new XClientMessageData
                        {
                            l = new[] { 1L, (long)atom, 0L, 0L, 0L }
                        }
                    };

                    XSendEvent(
                        display,
                        rootWindow,
                        false,
                        SubstructureRedirectMask | SubstructureNotifyMask,
                        ref clientMessage
                    );
                }
            }

            XCloseDisplay(display);
        }
        catch
        {
            // 忽略X11调用失败
        }
    }

    private void SetTransientFor(AVWindow transientWindow, AVWindow parentWindow)
    {
        try
        {
            var display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
                return;

            var transientId = GetX11WindowId(transientWindow);
            var parentId = GetX11WindowId(parentWindow);

            if (transientId != 0 && parentId != 0)
            {
                XSetTransientForHint(display, transientId, parentId);
            }

            XCloseDisplay(display);
        }
        catch
        {
            // 忽略X11调用失败
        }
    }

    private IntPtr GetX11WindowId(AVWindow window)
    {
        // 获取Avalonia窗口的X11窗口ID
        return window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    }

    // X11 DllImports
    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern UIntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(IntPtr display, IntPtr w, UIntPtr property,
        UIntPtr type, int format, int mode, ref UIntPtr data, int nElements);

    [DllImport("libX11.so.6")]
    private static extern int XSendEvent(IntPtr display, IntPtr w, bool propagate,
        long eventMask, ref XClientMessageEvent eventSend);

    [DllImport("libX11.so.6")]
    private static extern IntPtr DefaultRootWindow(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XSetTransientForHint(IntPtr display, IntPtr w, IntPtr propWindow);

    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(IntPtr display, IntPtr w, UIntPtr property,
        UIntPtr type, int format, int mode, IntPtr data, int nelements);

    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(IntPtr display, IntPtr w, UIntPtr property,
        UIntPtr type, int format, int mode, ref long data, int nelements);

    // X11常量
    private const int ClientMessage = 33;
    private const int PropModeReplace = 0;
    private const int SubstructureRedirectMask = (1 << 20);
    private const int SubstructureNotifyMask = (1 << 19);
    private const UIntPtr XaAtom = 4;

    // X11结构体
    [StructLayout(LayoutKind.Sequential)]
    private struct XClientMessageEvent
    {
        public int type;
        public ulong serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public long message_type;
        public int format;
        public XClientMessageData data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct XClientMessageData
    {
        [FieldOffset(0)]
        public long[] l;
        [FieldOffset(0)]
        public byte[] b;
        [FieldOffset(0)]
        public short[] s;
    }
}

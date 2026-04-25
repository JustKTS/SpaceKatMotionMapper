using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using PlatformAbstractions;
using AVWindow = Avalonia.Controls.Window;

namespace LinuxHelpers.Services.FloatingWindow;

/// <summary>
/// 浮动控制窗口 UI 工厂实现
/// 负责创建 UI 组件和设置 X11 防平铺属性
/// 通过 Func<object> 委托解耦对具体 View 类型的依赖
/// </summary>
public class FloatingControlWindowUIFactory(Func<object> createWindow) : IFloatingControlWindowUIFactory
{
    private readonly Func<object> _createWindow = createWindow ?? throw new ArgumentNullException(nameof(createWindow));

    public object CreateFloatingControlWindow()
    {
        return _createWindow();
    }

    public void SetAntiTilingProperties(object window)
    {
        if (window is not AVWindow avaloniaWindow) return;

        try
        {
            // 在X11环境下设置窗口类型
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

            // 设置窗口类型为UTILITY，防止被平铺
            SetX11WindowType(avaloniaWindow, "_NET_WM_WINDOW_TYPE_UTILITY");

            // 设置窗口状态
            SetX11WindowState(avaloniaWindow, "_NET_WM_STATE_SKIP_TASKBAR");
            SetX11WindowState(avaloniaWindow, "_NET_WM_STATE_SKIP_PAGER");
            SetX11WindowState(avaloniaWindow, "_NET_WM_STATE_ABOVE");
        }
        catch
        {
            // 如果设置失败，窗口仍然可以正常工作
        }
    }

    private void SetX11WindowType(AVWindow window, string windowType)
    {
        var display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            return;

        try
        {
            // 原子类型标识
            var atom = XInternAtom(display, windowType, false);
            if (atom == 0) return;
            var xid = GetXWindowId(window);
            if (xid == 0) return;
            // 设置窗口类型
            var atomLong = (long)atom;
            var ret = XChangeProperty(
                display,
                xid,
                XInternAtom(display, "_NET_WM_WINDOW_TYPE", false),
                XaAtom,
                32,
                PropModeReplace,
                ref atomLong,
                1
            );
        }
        finally
        {
            XCloseDisplay(display);
        }
    }

    private void SetX11WindowState(AVWindow window, string state)
    {
        var display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            return;

        try
        {
            var atom = XInternAtom(display, state, false);
            if (atom == 0) return;
            var xid = GetXWindowId(window);
            if (xid == 0) return;
            // 添加窗口状态
            var stateAtom = XInternAtom(display, "_NET_WM_STATE", false);
            var clientMessage = new XClientMessageEvent
            {
                type = ClientMessage,
                window = xid,
                message_type = (long)stateAtom,
                format = 32,
                data = new XClientMessageData
                {
                    l = new[] { 1L, (long)atom, 0L, 0L, 0L }
                }
            };

            var ret = XSendEvent(
                display,
                DefaultRootWindow(display),
                false,
                SubstructureRedirectMask | SubstructureNotifyMask,
                ref clientMessage
            );
        }
        finally
        {
            XCloseDisplay(display);
        }
    }

    private IntPtr GetXWindowId(AVWindow window)
    {
        // 在Avalonia中获取X11窗口ID的简化方法
        // 实际实现可能需要通过PlatformSpecific接口获取
        return window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    }

    // X11 DllImports
    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr? displayName);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6", CharSet = CharSet.Unicode)]
    private static extern UIntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(IntPtr display, IntPtr w, UIntPtr property,
        UIntPtr type, int format, int mode, ref long data, int nElements);

    [DllImport("libX11.so.6")]
    private static extern int XSendEvent(IntPtr display, IntPtr w, bool propagate,
        long eventMask, ref XClientMessageEvent eventSend);

    [DllImport("libX11.so.6")]
    private static extern IntPtr DefaultRootWindow(IntPtr display);

    // X11常量
    private const int ClientMessage = 33;
    private const int PropModeReplace = 0;
    private const int SubstructureRedirectMask = (1 << 20);
    private const int SubstructureNotifyMask = (1 << 19);
    private const UIntPtr XaAtom = 4;

    // X11结构体（简化版）
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

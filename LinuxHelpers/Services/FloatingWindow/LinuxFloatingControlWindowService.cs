using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;
using PlatformAbstractions;
using Serilog;
using AVWindow = Avalonia.Controls.Window;

namespace LinuxHelpers.Services.FloatingWindow;

/// <summary>
/// Linux平台的浮动控制窗口服务实现
/// 在不支持系统托盘的Linux桌面环境下显示浮动控制窗口
/// </summary>
public class LinuxFloatingControlWindowService(IFloatingControlWindowUIFactory uiFactory)
    : IFloatingControlWindowService
{
    private readonly IFloatingControlWindowUIFactory _uiFactory = uiFactory ?? throw new ArgumentNullException(nameof(uiFactory));
    private AVWindow? _mainWindow;
    private object? _floatingWindow;

    public bool IsSupported => true;
    public bool IsVisible { get; private set; }

    public event EventHandler? MainWindowRestoreRequested;
    public event EventHandler? ApplicationExitRequested;

    public bool SetMainWindow(object mainWindow)
    {
        _mainWindow = mainWindow as AVWindow;
        return _mainWindow != null;
    }

    public void Show(object mainWindow)
    {
        if (mainWindow is not AVWindow avaloniaWindow) return;
        _mainWindow = avaloniaWindow;

        if (_floatingWindow == null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _floatingWindow = _uiFactory.CreateFloatingControlWindow();
                if (_floatingWindow is AVWindow window)
                {
                    _uiFactory.SetAntiTilingProperties(window);
                }
            });
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_floatingWindow is not AVWindow window || window.IsVisible) return;
            window.Show();
            IsVisible = true;
        });
    }

    public void Hide()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_floatingWindow is not AVWindow window || !window.IsVisible) return;
            window.Hide();
            IsVisible = false;
        });
    }

    public void Toggle(object mainWindow)
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            Show(mainWindow);
        }
    }

    public async void RestoreMainWindow()
    {
        if (_mainWindow == null)
        {
            // 主窗口无效，触发退出
            ApplicationExitRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                // 确保窗口可见
                _mainWindow.ShowInTaskbar = true;
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Show();

                // 使用临时Topmost确保窗口置顶
                _mainWindow.Topmost = true;
                _mainWindow.Activate();

                // 给窗口管理器一些时间处理
                await Task.Delay(50);

                // 移除Topmost属性
                _mainWindow.Topmost = false;

                // 额外的激活尝试
                _mainWindow.Activate();
            }
            catch (Exception ex)
            {
                // 记录错误但继续执行
                Log.Warning(ex, "[{Service}] Error restoring main window: {Message}", nameof(LinuxFloatingControlWindowService), ex.Message);
            }
        });

        // 延迟隐藏浮动窗口，确保主窗口完全恢复
        await Task.Delay(100);
        Hide();

        MainWindowRestoreRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ExitApplication()
    {
        ApplicationExitRequested?.Invoke(this, EventArgs.Empty);
    }

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

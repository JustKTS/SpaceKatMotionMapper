using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PlatformAbstractions;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class FloatingControlWindow : UrsaWindow
{
    private readonly IFloatingControlWindowService? _service;

    public FloatingControlWindow()
    {
        InitializeComponent();
        _service = App.GetRequiredService<IFloatingControlWindowService>();

        // 设置窗口初始位置（右下角）
        SetInitialPosition();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // 绑定服务事件
        if (_service != null)
        {
            _service.SetMainWindow(App.GetRequiredService<MainWindow>());
            _service.MainWindowRestoreRequested += OnMainWindowRestoreRequested;
            _service.ApplicationExitRequested += OnApplicationExitRequested;
        }

        // 添加拖拽事件处理
        PointerPressed += InputElement_OnPointerPressed;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // 解绑服务事件
        if (_service != null)
        {
            _service.MainWindowRestoreRequested -= OnMainWindowRestoreRequested;
            _service.ApplicationExitRequested -= OnApplicationExitRequested;
        }

        // 移除拖拽事件处理
        PointerPressed -= InputElement_OnPointerPressed;
    }

    private void SetInitialPosition()
    {
        if (Screens.Primary is not { } screen) return;
        var workingArea = screen.WorkingArea;
        const int margin = 20;
        Position = new PixelPoint(
            workingArea.X + workingArea.Width - (int)Width - margin,
            workingArea.Y + workingArea.Height - (int)Height - margin
        );
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnAppIconClick(object? sender, RoutedEventArgs e)
    {
        _service?.RestoreMainWindow();
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void OnRestoreClick(object? sender, RoutedEventArgs e)
    {
        _service?.RestoreMainWindow();
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        _service?.ExitApplication();
    }

    private void OnMainWindowRestoreRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(Hide);
    }

    private void OnApplicationExitRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            App.GetRequiredService<MainWindow>().Close();
            Close();
        });
    }
}
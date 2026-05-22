using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using PlatformAbstractions;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.NavVMs;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace SpaceKatMotionMapper.Views;

public partial class MainWindow : UrsaWindow
{
    private readonly WindowNotificationManager _manager;
    private readonly IPlatformMinimizeService _minimizeService;
    public const string LocalHost = "LocalHost";

    public MainWindow()
    {
        DataContext = App.GetRequiredService<NavViewModel>();
        _minimizeService = App.GetRequiredService<IPlatformMinimizeService>();

        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        var topLevel = GetTopLevel(this);
        _manager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 3,
            Position = NotificationPosition.BottomCenter
        };

        // 订阅最小化服务事件
        _minimizeService.WindowMinimized += OnWindowMinimized;
        _minimizeService.WindowRestored += OnWindowRestored;
        _minimizeService.WindowHidden += OnWindowHidden;

        WeakReferenceMessenger.Default.Register<PopupNotificationData, string>(this, "PopUpNotification",
            PopupNotification
        );
    }


    private async void PopupNotification(object sender, PopupNotificationData e)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var notification = new Notification
            {
                Content = e.Message,
                ShowIcon = true,
                Title = e.NotificationType.ToString(),
                Type = e.NotificationType
            };

            // 显示通知
            _manager.Show(notification);
        });
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != WindowStateProperty) return;
        var newState = (WindowState?)e.NewValue;

        // 使用平台最小化服务处理最小化
        if (newState == WindowState.Minimized)
        {
            _minimizeService.MinimizeWindow(this);
        }
    }

    private void OnWindowMinimized(object? sender, object window)
    {
        // 窗口被最小化时的处理
        if (Equals(window, this))
        {
            // 可以在这里添加额外的最小化处理逻辑
        }
    }

    private void OnWindowRestored(object? sender, object window)
    {
        // 窗口被恢复时的处理
        if (!Equals(window, this)) return;
        ShowInTaskbar = true;
        WindowState = WindowState.Normal;
    }

    private void OnWindowHidden(object? sender, object window)
    {
        // 窗口被隐藏时的处理
        if (Equals(window, this))
        {
            ShowInTaskbar = false;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        HideNativeDecorations();
        var swOnLoaded = Stopwatch.StartNew();
        OnStartOrCloseFunctions.LoadOnMainWindowLoaded();
        var swNav = Stopwatch.StartNew();
        var navVm = App.GetRequiredService<NavViewModel>();
        navVm.OnNavigation(navVm, typeof(MainView).FullName!);
        swNav.Stop();
        swOnLoaded.Stop();

        if (Program.ShouldMinimizeToTray)
        {
            _minimizeService.HideToBackground(this);
        }
    }

    private void HideNativeDecorations()
    {
        var decorations = this.GetLogicalDescendants()
            .OfType<WindowDrawnDecorations>()
            .FirstOrDefault();
        if (decorations is null) return;

        var overlayPanel = decorations.GetLogicalDescendants()
            .OfType<StackPanel>()
            .FirstOrDefault(s => s.Name == "PART_OverlayPanel");
        if (overlayPanel is not null)
        {
            overlayPanel.IsVisible = false;
        }

        var closeButton = decorations.GetLogicalDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.Name == "PART_CloseButton");
        if (closeButton is not null)
        {
            closeButton.IsVisible = false;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        OnStartOrCloseFunctions.OnMainWindowClosing();
        base.OnClosing(e);
    }
}
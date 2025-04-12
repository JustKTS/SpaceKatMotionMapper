using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.NavVMs;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace SpaceKatMotionMapper.Views;

public partial class MainWindow : UrsaWindow
{
    private readonly WindowNotificationManager _manager;
    public const string LocalHost = "LocalHost";

    public MainWindow()
    {
        DataContext = App.GetRequiredService<NavViewModel>();
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        var topLevel = GetTopLevel(this);
        _manager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 3,
            Position = NotificationPosition.BottomCenter
        };

        WeakReferenceMessenger.Default.Register<PopupNotificationData, string>(this, "PopUpNotification",
            PopupNotification
        );
    }


    private void PopupNotification(object sender, PopupNotificationData e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _manager.Show(
                new Notification()
                {
                    Content = e.Message,
                    ShowIcon = true,
                    Title = e.NotificationType.ToString(),
                    Type = e.NotificationType
                });
        });
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Window.WindowStateProperty) return;
        var newState = (WindowState?)e.NewValue;
        ShowInTaskbar = newState != WindowState.Minimized;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        OnStartOrCloseFunctions.LoadOnMainWindowLoaded();
        var navVm = App.GetRequiredService<NavViewModel>();
        navVm.OnNavigation(navVm, typeof(MainView).FullName!);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        OnStartOrCloseFunctions.OnMainWindowClosing();
        base.OnClosing(e);
    }
}
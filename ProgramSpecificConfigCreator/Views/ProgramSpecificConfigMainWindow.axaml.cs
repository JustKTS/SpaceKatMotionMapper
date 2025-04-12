using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ProgramSpecificConfigCreator.Helpers;
using ProgramSpecificConfigCreator.ViewModels;
using SpaceKat.Shared.Models;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace ProgramSpecificConfigCreator.Views;

public partial class ProgramSpecificConfigMainWindow : UrsaWindow
{
    private readonly WindowNotificationManager _manager;

    public const string LocalHost = "ProgramSpecificConfigWindow";

    public ProgramSpecificConfigMainWindow()
    {
        DataContext = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecMainViewModel>();
        InitializeComponent();

        var topLevel = GetTopLevel(this);
        _manager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 3,
            Position = NotificationPosition.TopCenter
        };
    }


    public async Task PopupNotification(PopupNotificationData e)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _manager.Show(
                new Notification
                {
                    Content = e.Message,
                    ShowIcon = true,
                    Title = e.NotificationType.ToString(),
                    Type = e.NotificationType
                });
        });
    }

    public async Task<bool> ChangeIsLoadingAsync(bool isLoading)
    {
        return await Dispatcher.UIThread.InvokeAsync(() => LoadingContainer.IsLoading = isLoading);
    }
}
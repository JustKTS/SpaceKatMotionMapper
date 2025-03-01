using Avalonia.Controls;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class TimeAndDeadZoneSettingView : UrsaView
{
    public TimeAndDeadZoneSettingView()
    {
        DataContext = App.GetRequiredService<TimeAndDeadZoneSettingViewModel>();
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }
}
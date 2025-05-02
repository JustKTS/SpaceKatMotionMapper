using Avalonia.Controls;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class TimeAndDeadZoneSettingView : UserControl
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
using Avalonia.Controls;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class TimeAndDeadZoneSettingView : UserControl
{
    public TimeAndDeadZoneSettingView()
    {
        DataContext = App.GetRequiredService<TimeAndDeadZoneSettingViewModel>();
        InitializeComponent();
    }
}
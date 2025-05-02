using Avalonia.Controls;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class DeadZoneConfigView : UserControl
{
    public DeadZoneConfigView()
    {
        DataContext = App.GetRequiredService<DeadZoneConfigViewModel>();
        InitializeComponent();
    }
}
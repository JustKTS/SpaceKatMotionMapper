using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class DeadZoneConfigView : UrsaView
{
    public DeadZoneConfigView()
    {
        DataContext = App.GetRequiredService<DeadZoneConfigViewModel>();
        InitializeComponent();
    }
}
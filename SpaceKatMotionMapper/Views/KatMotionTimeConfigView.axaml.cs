using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class KatMotionTimeConfigView : UrsaView
{
    public KatMotionTimeConfigView()
    {
        DataContext = App.GetRequiredService<MotionTimeConfigViewModel>();
        InitializeComponent();
    }
}
using Avalonia.Controls;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class ConnectAndEnableView : UserControl
{
    public ConnectAndEnableView()
    {
        DataContext = App.GetRequiredService<ConnectAndEnableViewModel>();
        InitializeComponent();
    }
}
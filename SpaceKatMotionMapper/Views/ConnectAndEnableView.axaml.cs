using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class ConnectAndEnableView : UrsaView
{
    public ConnectAndEnableView()
    {
        DataContext = App.GetRequiredService<ConnectAndEnableViewModel>();
        InitializeComponent();
    }
}
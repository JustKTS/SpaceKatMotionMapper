using Avalonia.Interactivity;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class MainView : UrsaView
{
    public MainView()
    {
        DataContext = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
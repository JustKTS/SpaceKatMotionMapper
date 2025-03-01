using CommunityToolkit.Mvvm.Messaging;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class MainView : UrsaView
{
    public MainView()
    {
        DataContext = App.GetService<MainViewModel>();
        InitializeComponent();
        // WeakReferenceMessenger.Default.Register<object, string, string>(this, "KnobConnectionResult", (o, s) =>
        // {
        //     if (s == "failed")
        //     {
        //         ConnectBtn.IsChecked = false;
        //     }
        // });
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class AutoDisableConfigView : UrsaView
{
    public AutoDisableConfigView()
    {
        DataContext = App.GetRequiredService<AutoDisableViewModel>();
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        (DataContext as AutoDisableViewModel)?.LoadInfos();
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class AutoDisableConfigView : UserControl
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
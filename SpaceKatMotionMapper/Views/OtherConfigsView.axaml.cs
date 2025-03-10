using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class OtherConfigsView : UrsaView
{
    private readonly OtherConfigsViewModel _viewModel;
    private bool _isFirstLoaded = false;
    public OtherConfigsView()
    {
        _viewModel =  App.GetService<OtherConfigsViewModel>();
        DataContext = _viewModel;
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (!_isFirstLoaded) return;
        _viewModel.ReloadConfigGroupsFromSysConfCommand.Execute(null);
        _isFirstLoaded = false;
    }
}
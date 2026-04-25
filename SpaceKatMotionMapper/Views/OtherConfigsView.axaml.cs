using Avalonia.Interactivity;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class OtherConfigsView : UrsaView
{
    private readonly OtherConfigsViewModel _viewModel;
    private bool _isFirstLoaded = true;
    public OtherConfigsView()
    {
        _viewModel =  App.GetService<OtherConfigsViewModel>();
        _viewModel.AddCommand.Execute(null); // 添加一个默认配置
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
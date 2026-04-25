using Avalonia.Controls;
using Avalonia.Interactivity;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class KatMotionConfigItemView : UserControl
{
    public KatMotionConfigItemView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is KatMotionViewModel vm &&
            vm.Parent.Configs.Count > 0 &&
            vm.Parent.Configs[0] == vm)
        {
            ConfigExpander.IsExpanded = true;
        }
    }
}

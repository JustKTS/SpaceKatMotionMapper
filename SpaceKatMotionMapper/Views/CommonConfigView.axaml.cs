using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class CommonConfigView : UserControl
{
    public CommonConfigView()
    {
        DataContext = App.GetService<CommonConfigViewModel>();
        InitializeComponent();
        var view = new KatActionConfigView
        {
            DataContext = ((CommonConfigViewModel)DataContext).DefaultKatActionConfig
        };
        ScrollViewer.Content = view;
    }
}
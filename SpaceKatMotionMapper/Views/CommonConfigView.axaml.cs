using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class CommonConfigView : UserControl
{
    public CommonConfigView()
    {
        InitializeComponent();
        DataContext = App.GetService<CommonConfigViewModel>();
    }
}
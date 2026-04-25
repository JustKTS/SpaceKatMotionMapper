using Avalonia.Controls;
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
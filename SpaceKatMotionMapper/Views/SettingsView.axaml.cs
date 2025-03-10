using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        DataContext = App.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
    }
}
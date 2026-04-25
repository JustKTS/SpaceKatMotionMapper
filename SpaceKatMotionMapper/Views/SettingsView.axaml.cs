using Avalonia.Controls;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        DataContext = App.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        HotKeyTextBoxHelper.Register(HotKeyTextBox);
    }
}
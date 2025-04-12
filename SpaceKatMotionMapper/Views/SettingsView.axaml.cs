using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        DataContext = App.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
    }
    
    private void HotKeyTextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            textBox.Text = e.Key.ToVirtualKeyCode().GetWrappedName();
        });
    }
}
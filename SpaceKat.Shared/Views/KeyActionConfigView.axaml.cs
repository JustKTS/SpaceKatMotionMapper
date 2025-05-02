using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using SpaceKat.Shared.Helpers;

namespace SpaceKat.Shared.Views;

public partial class KeyActionConfigView : UserControl
{
    public KeyActionConfigView()
    {
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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.ViewModels;

namespace SpaceKat.Shared.Views;

public partial class CombinationKeyActionConfigControl : UserControl
{
    public CombinationKeyActionConfigControl()
    {
        InitializeComponent();
    }

    private void HotKeyTextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        Dispatcher.UIThread.InvokeAsync(() => { textBox.Text = e.Key.ToVirtualKeyCode().GetWrappedName(); });
    }
}
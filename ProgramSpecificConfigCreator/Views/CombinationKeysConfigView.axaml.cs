using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ProgramSpecificConfigCreator.ViewModels;
using SpaceKat.Shared.Helpers;

namespace ProgramSpecificConfigCreator.Views;

public partial class CombinationKeysConfigView : UserControl
{
    public CombinationKeysConfigView()
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
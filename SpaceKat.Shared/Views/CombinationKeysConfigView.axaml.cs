using Avalonia.Controls;
using SpaceKat.Shared.Helpers;

namespace SpaceKat.Shared.Views;

public partial class CombinationKeysConfigView : UserControl
{
    public CombinationKeysConfigView()
    {
        InitializeComponent();
        HotKeyTextBoxHelper.Register(HotKeyTextBox);
    }
}
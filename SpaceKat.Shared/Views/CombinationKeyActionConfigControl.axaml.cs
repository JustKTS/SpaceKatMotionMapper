using Avalonia.Controls;
using SpaceKat.Shared.Helpers;

namespace SpaceKat.Shared.Views;

public partial class CombinationKeyActionConfigControl : UserControl
{
    public CombinationKeyActionConfigControl()
    {
        InitializeComponent();
        HotKeyTextBoxHelper.Register(HotKeyTextBox);
    }
}
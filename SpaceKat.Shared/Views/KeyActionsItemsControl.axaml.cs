using Avalonia.Controls;
using Avalonia.Input;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.ViewModels;

namespace SpaceKat.Shared.Views;

public partial class KeyActionsItemsControl : UserControl
{
    public KeyActionsItemsControl()
    {
        InitializeComponent();
    }

    private void KeySelectionCBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not Ursa.Controls.AutoCompleteBox autoCompleteBox) return;

        var selectedKey = e.AddedItems.OfType<string>().FirstOrDefault()
                          ?? autoCompleteBox.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(selectedKey)) return;

        autoCompleteBox.Text = selectedKey;
        if (autoCompleteBox.DataContext is not KeyActionWithCommandViewModel viewModel) return;

        viewModel.Key = selectedKey;
    }
}
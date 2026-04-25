using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Helpers;

public static class HotKeyTextBoxHelper
{
    public static void Register(TextBox textBox)
    {
        textBox.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        TryHandleKeyDown(sender, e);
    }

    public static bool TryHandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return false;

        var wrappedKey = e.Key.ToKeyCodeWrapper();
        if (wrappedKey == KeyCodeWrapper.NONE && e.Key != Key.None) return false;

        e.Handled = true;
        textBox.Text = wrappedKey.GetWrappedName();
        textBox.CaretIndex = textBox.Text?.Length ?? 0;
        return true;
    }
}


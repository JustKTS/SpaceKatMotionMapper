using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SpaceKat.Shared.Helpers;

public class HotKeyTextBox
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<HotKeyTextBox, TextBox, bool>("IsEnabled");

    static HotKeyTextBox()
    {
        IsEnabledProperty.Changed
            .AddClassHandler<TextBox, bool>((textBox, e) =>
            {
                if (e.NewValue.Value)
                    HotKeyTextBoxHelper.Register(textBox);
            });
    }

    public static bool GetIsEnabled(TextBox textBox) => textBox.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(TextBox textBox, bool value) => textBox.SetValue(IsEnabledProperty, value);
}

using Avalonia;
using Avalonia.Controls;

namespace SpaceKat.Shared.Views;

public partial class KeyActionConfigV2Control : UserControl
{
    public static readonly StyledProperty<bool> IsCustomDescriptionProperty =
        AvaloniaProperty.Register<KeyActionConfigV2Control, bool>(nameof(IsCustomDescription), defaultValue:false);

    public bool IsCustomDescription
    {
        get => GetValue(IsCustomDescriptionProperty);
        set => SetValue(IsCustomDescriptionProperty, value);
    }
    
    public KeyActionConfigV2Control()
    {
        InitializeComponent();
    }
}
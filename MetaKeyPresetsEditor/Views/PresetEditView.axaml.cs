using Avalonia.Controls;
using MetaKeyPresetsEditor.ViewModels;

namespace MetaKeyPresetsEditor.Views;

public partial class PresetEditView : UserControl
{
    public PresetEditView()
    {
        DataContext = App.GetRequiredService<ProgramSpecificConfigViewModel>();
        InitializeComponent();
    }
}
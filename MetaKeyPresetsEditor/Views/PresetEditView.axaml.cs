using Avalonia.Controls;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MetaKeyPresetsEditor.Views;

public partial class PresetEditView : UserControl
{
    public PresetEditView()
    {
        DataContext = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
        InitializeComponent();
    }
}
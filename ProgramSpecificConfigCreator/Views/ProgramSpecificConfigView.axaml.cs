using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ProgramSpecificConfigCreator.Helpers;
using ProgramSpecificConfigCreator.ViewModels;

namespace ProgramSpecificConfigCreator.Views;

public partial class ProgramSpecificConfigView : UserControl
{
    public ProgramSpecificConfigView()
    {
        DataContext = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();
        InitializeComponent();
    }
}
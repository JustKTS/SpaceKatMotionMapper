using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Ursa.Controls;

namespace MetaKeyPresetsEditor.Views;

public partial class PresetsEditorMainView : UserControl
{
    public PresetsEditorMainView()
    {
        DataContext = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecMainViewModel>();
        InitializeComponent();
    }
    public async Task<bool> ChangeIsLoadingAsync(bool isLoading)
    {
        return await Dispatcher.UIThread.InvokeAsync(() => LoadingContainer.IsLoading = isLoading);
    }
    
}
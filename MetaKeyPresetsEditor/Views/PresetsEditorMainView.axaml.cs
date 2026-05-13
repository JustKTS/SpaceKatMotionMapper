using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MetaKeyPresetsEditor.ViewModels;

namespace MetaKeyPresetsEditor.Views;

public partial class PresetsEditorMainView : UserControl
{
    public PresetsEditorMainView()
    {
        DataContext = App.GetRequiredService<ProgramSpecMainViewModel>();
        InitializeComponent();
    }
    public async Task<bool> ChangeIsLoadingAsync(bool isLoading)
    {
        return await Dispatcher.UIThread.InvokeAsync(() => LoadingContainer.IsLoading = isLoading);
    }
    
}
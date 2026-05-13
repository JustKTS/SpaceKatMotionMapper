using System.Threading.Tasks;
using MetaKeyPresetsEditor.ViewModels;
using MetaKeyPresetsEditor.Views;

namespace MetaKeyPresetsEditor.Services;

public class UiInteractService(PresetsEditorMainView view, ProgramSpecificConfigViewModel mainVm) : IUiInteractService
{
    public async Task ChangeConfigLoadingAsync(bool isLoading)
    {
        await view.ChangeIsLoadingAsync(isLoading);
    }

    public async Task ChangeConfigNameAsync(string configName)
    {
        await mainVm.ChangeConfigNameAsync(configName);
    }
}
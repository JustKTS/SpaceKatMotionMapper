using System.Threading.Tasks;
using Avalonia.Threading;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.ViewModels;
using MetaKeyPresetsEditor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MetaKeyPresetsEditor.Services;

public class UiInteractService : IUiInteractService
{
    private readonly PresetsEditorMainView
        _view = DIHelper.GetServiceProvider().GetRequiredService<PresetsEditorMainView>();

    private readonly ProgramSpecificConfigViewModel _mainVm = DIHelper.GetServiceProvider().GetRequiredService<ProgramSpecificConfigViewModel>();

    public async Task ChangeConfigLoadingAsync(bool isLoading)
    {
        await _view.ChangeIsLoadingAsync(isLoading);
    }

    public async Task ChangeConfigNameAsync(string configName)
    {
        await _mainVm.ChangeConfigNameAsync(configName);
    }
}
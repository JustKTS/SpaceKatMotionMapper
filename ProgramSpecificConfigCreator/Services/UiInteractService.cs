using System.Threading.Tasks;
using Avalonia.Threading;
using ProgramSpecificConfigCreator.ViewModels;
using ProgramSpecificConfigCreator.Views;

namespace ProgramSpecificConfigCreator.Services;

public class UiInteractService : IUiInteractService
{
    private readonly ProgramSpecificConfigMainWindow
        _window = App.GetRequiredService<ProgramSpecificConfigMainWindow>();

    private readonly ProgramSpecificConfigViewModel _mainVm = App.GetRequiredService<ProgramSpecificConfigViewModel>();

    public async Task ChangeConfigLoadingAsync(bool isLoading)
    {
        await _window.ChangeIsLoadingAsync(isLoading);
    }

    public async Task ChangeProgramNameAsync(string programName)
    {
        await _mainVm.ChangeProgramNameAsync(programName);
    }
}
using System.Threading.Tasks;

namespace ProgramSpecificConfigCreator.Services;

public interface IUiInteractService
{
    Task ChangeConfigLoadingAsync(bool isLoading);
    Task ChangeProgramNameAsync(string programName);
}
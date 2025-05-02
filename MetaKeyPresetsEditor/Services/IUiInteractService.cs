using System.Threading.Tasks;

namespace MetaKeyPresetsEditor.Services;

public interface IUiInteractService
{
    Task ChangeConfigLoadingAsync(bool isLoading);
    Task ChangeConfigNameAsync(string configName);
}
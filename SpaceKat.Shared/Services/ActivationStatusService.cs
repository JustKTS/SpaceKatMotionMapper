using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class ActivationStatusService(ILocalSettingsService localSettingsService)
{
    private Dictionary<Guid, bool> _activationStatus = [];
    private const string SaveToken = "ActivationStatus";
    private ManualResetEventSlim _isLoadedEvent = new(false);

    public void WaitForActivationStatusLoaded()
    {
        Task.Run(async () => await LoadActivationStatusAsync());
        _isLoadedEvent.Wait();
    }

    private async Task LoadActivationStatusAsync()
    {
        try
        {
            var ret = await localSettingsService.ReadSettingAsync<Dictionary<Guid, bool>>(SaveToken);
            _activationStatus = ret ?? new Dictionary<Guid, bool>();
            _isLoadedEvent.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void SaveActivationStatus()
    {
        _ = localSettingsService.SaveSettingAsync(SaveToken, _activationStatus);
    }

    public void SetActivationStatus(Guid configGroupId, bool isActivated)
    {
        _activationStatus[configGroupId] = isActivated;
    }

    public bool IsConfigGroupActivated(Guid configGroupId)
    {
        try
        {
            return _activationStatus.ContainsKey(configGroupId) && _activationStatus[configGroupId];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public void DeleteActivationStatus(Guid configGroupId)
    {
        _activationStatus.Remove(configGroupId);
    }
}
using System.Text.Json.Serialization;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class ActivationStatusService
{
    private Dictionary<Guid, bool> _activationStatus = [];
    private const string SaveToken = "ActivationStatus";
    private readonly ManualResetEventSlim _isLoadedEvent = new(false);
    private readonly ILocalSettingsService _localSettingsService;

    public ActivationStatusService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        WaitForActivationStatusLoaded();
    }
    
    public void WaitForActivationStatusLoaded()
    {
        Task.Run(async () => await LoadActivationStatusAsync());
        _isLoadedEvent.Wait();
    }

    private async Task LoadActivationStatusAsync()
    {
        try
        {
            var ret = await _localSettingsService.ReadSettingAsync<Dictionary<Guid, bool>>(SaveToken);
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
        _ = _localSettingsService.SaveSettingAsync(SaveToken, _activationStatus);
    }

    public void SetActivationStatus(Guid configGroupId, bool isActivated)
    {
        _activationStatus[configGroupId] = isActivated;
        SaveActivationStatus();
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
        SaveActivationStatus();
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<Guid, bool>))]
public partial class ActivationStatusJsonSgContext : JsonSerializerContext
{
}
using System.Text.Json.Serialization;
using Serilog;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class ActivationStatusService : IActivationStatusService
{
    private Dictionary<Guid, bool> _activationStatus = [];
    private const string SaveToken = "ActivationStatus";
    private readonly ManualResetEventSlim _isLoadedEvent = new(false);
    private readonly ILocalSettingsService _localSettingsService;
    private Exception? _loadException;

    public ActivationStatusService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        WaitForActivationStatusLoaded();
    }

    public void WaitForActivationStatusLoaded()
    {
        Task.Run(async () =>
        {
            try
            {
                await LoadActivationStatusAsync();
            }
            catch (Exception e)
            {
                _loadException = e;
                _isLoadedEvent.Set();
            }
        });
        _isLoadedEvent.Wait();

        if (_loadException is not null)
            throw _loadException;
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
            Log.Error(e, "[{Service}] Failed to load activation status", nameof(ActivationStatusService));
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
            Log.Error(e, "[{Service}] Failed to check activation status for {ConfigGroupId}", nameof(ActivationStatusService), configGroupId);
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
using System;
using System.Text.Json.Serialization;
using System.Threading;
using Serilog;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class ActivationStatusService : IActivationStatusService, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ActivationStatusService>();

    private Dictionary<Guid, bool> _activationStatus = [];
    private const string SaveToken = "ActivationStatus";
    private readonly ManualResetEvent _isLoadedEvent = new(false);
    private readonly ILocalSettingsService _localSettingsService;
    private Exception? _loadException;

    public ActivationStatusService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            await LoadActivationStatusAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _loadException = e;
        }
    }

    public void WaitForActivationStatusLoaded()
    {
        _isLoadedEvent.WaitOne(TimeSpan.FromSeconds(5));

        if (_loadException is not null)
            throw _loadException;
    }

    private async Task LoadActivationStatusAsync()
    {
        try
        {
            var ret = await _localSettingsService.ReadSettingAsync<Dictionary<Guid, bool>>(SaveToken).ConfigureAwait(false);
            _activationStatus = ret ?? new Dictionary<Guid, bool>();
            _isLoadedEvent.Set();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load activation status");
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
            Log.Error(e, "Failed to check activation status for {ConfigGroupId}", configGroupId);
            return false;
        }
    }

    public void DeleteActivationStatus(Guid configGroupId)
    {
        _activationStatus.Remove(configGroupId);
        SaveActivationStatus();
    }

    public void Dispose()
    {
        _isLoadedEvent.Dispose();
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<Guid, bool>))]
public partial class ActivationStatusJsonSgContext : JsonSerializerContext
{
}

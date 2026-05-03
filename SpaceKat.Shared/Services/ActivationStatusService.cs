using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using Serilog;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class ActivationStatusService : IActivationStatusService, IDisposable
{
    private static readonly object _startupLogLock = new();
    private static void StartupLog(string msg)
    {
        var line = $"{DateTime.Now:HH:mm:ss.fff} [THREAD:{Environment.CurrentManagedThreadId}] {msg}\n";
        lock (_startupLogLock)
        {
            using var fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_debug.log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            fs.Write(System.Text.Encoding.UTF8.GetBytes(line));
            fs.Flush(false);
        }
    }

    private Dictionary<Guid, bool> _activationStatus = [];
    private const string SaveToken = "ActivationStatus";
    private readonly ManualResetEvent _isLoadedEvent = new(false);
    private readonly ILocalSettingsService _localSettingsService;
    private Exception? _loadException;

    public ActivationStatusService(ILocalSettingsService localSettingsService)
    {
        StartupLog("ActivationStatusService.ctor: START");
        _localSettingsService = localSettingsService;
        WaitForActivationStatusLoaded();
        StartupLog("ActivationStatusService.ctor: END");
    }

    public void WaitForActivationStatusLoaded()
    {
        StartupLog("WaitForActivationStatusLoaded: BEFORE Task.Run");
        Task.Run(async () =>
        {
            StartupLog("WaitForActivationStatusLoaded: Task.Run ENTERED");
            try
            {
                await LoadActivationStatusAsync();
                StartupLog("WaitForActivationStatusLoaded: LoadActivationStatusAsync completed successfully");
            }
            catch (Exception e)
            {
                _loadException = e;
                _isLoadedEvent.Set();
                StartupLog($"WaitForActivationStatusLoaded: Task.Run EXCEPTION: {e.Message}");
            }
        });
        StartupLog("WaitForActivationStatusLoaded: BEFORE _isLoadedEvent.WaitOne()");
        var signalled = _isLoadedEvent.WaitOne(TimeSpan.FromSeconds(5));
        if (!signalled)
            StartupLog("WaitForActivationStatusLoaded: _isLoadedEvent.WaitOne() TIMEOUT after 5s!");
        else
            StartupLog("WaitForActivationStatusLoaded: _isLoadedEvent.WaitOne() returned OK");
        StartupLog("WaitForActivationStatusLoaded: AFTER _isLoadedEvent.WaitOne()");

        if (_loadException is not null)
            throw _loadException;
    }

    private async Task LoadActivationStatusAsync()
    {
        try
        {
            var ret = await _localSettingsService.ReadSettingAsync<Dictionary<Guid, bool>>(SaveToken);
            _activationStatus = ret ?? new Dictionary<Guid, bool>();
            StartupLog("LoadActivationStatusAsync: BEFORE _isLoadedEvent.Set()");
            _isLoadedEvent.Set();
            StartupLog("LoadActivationStatusAsync: AFTER _isLoadedEvent.Set()");
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

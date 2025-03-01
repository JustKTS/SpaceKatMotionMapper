
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class ActivationStatusService(ILocalSettingsService localSettingsService)
{
    private Dictionary<Guid, bool> _activationStatus = [];
    private const string SaveToken = "ActivationStatus";
    private bool _isLoaded = false;

    public void WaitForActivationStatusLoaded()
    {
        Task.Run(async () => await LoadActivationStatusAsync());
        while (!_isLoaded)
        {
            Thread.Sleep(100);
        }
    }

    private async Task LoadActivationStatusAsync()
    {
        try
        {
            var ret = await localSettingsService.ReadSettingAsync<Dictionary<Guid, bool>>(SaveToken);
            _activationStatus = ret ?? new Dictionary<Guid, bool>();
            _isLoaded = true;
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
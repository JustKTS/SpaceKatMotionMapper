using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKatMotionMapper.States;

public partial class GlobalStates : ObservableObject, IGlobalStates
{
    private readonly ILocalSettingsService _localSettingsService;

    public GlobalStates(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    # region 全局信息
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _isMapperEnable;
    [ObservableProperty] private bool _isTransparentInfoEnable = true;

    public const string IsMapperEnableKey = "IsMapperEnable";

    public event EventHandler<bool>? IsConnectionChanged;
    public event EventHandler<bool>? IsMapperEnableChanged;
    public event EventHandler<bool>? IsTransparentInfoEnableChanged;

    partial void OnIsConnectedChanged(bool value)
    {
        IsConnectionChanged?.Invoke(this, value);
    }
    
    partial void OnIsMapperEnableChanged(bool value)
    {
        _localSettingsService.SaveSettingAsync(IsMapperEnableKey, value);
        IsMapperEnableChanged?.Invoke(this, value);
    }
    
    partial void OnIsTransparentInfoEnableChanged(bool value)
    {
        IsTransparentInfoEnableChanged?.Invoke(this, value);
    }
    
    # endregion
}
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LanguageExt;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.States;

public partial class GlobalStates : ObservableObject
{
    # region 全局信息
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _isMapperEnable;
    [ObservableProperty] private bool _isTransparentInfoEnable;

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
        App.GetRequiredService<ILocalSettingsService>().SaveSettingAsync(IsMapperEnableKey, value);
        IsMapperEnableChanged?.Invoke(this, value);
    }
    
    partial void OnIsTransparentInfoEnableChanged(bool value)
    {
        IsTransparentInfoEnableChanged?.Invoke(this, value);
    }
    
    # endregion
}
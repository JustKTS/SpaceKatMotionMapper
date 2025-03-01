using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;


namespace SpaceKatMotionMapper.ViewModels;

public partial class MotionTimeConfigViewModel(
    KatActionRecognizeService katActionRecognizeService,
    KatMotionTimeConfigService katMotionTimeConfigService)
    : ViewModelBase
{
    [ObservableProperty] private bool _isDefault = true;

    [ObservableProperty] private Guid _id;

    private KatMotionTimeConfigs _configs = new(false);

    public ObservableCollection<MotionTimeSingleConfigViewModel> ConfigViewModels { get; } = [];

#if DEBUG
    // Rider XAML设计器问题，不支持没有空构造函数的预览
    public MotionTimeConfigViewModel() : this(null!, null!)
    {
    }
#endif

    public void UpdateById(Guid id)
    {
        IsDefault = false;
        Id = id;
    }
    
    public void UpdateByDefault()
    {
        IsDefault = true;
    }

    #region 时间参数获取与设置

    [ObservableProperty] private bool _useUnifySetting;

    partial void OnUseUnifySettingChanged(bool value)
    {
        ReloadConfigs();
    }

    [RelayCommand]
    private void LoadMotionTimeConfigs()
    {
        var configs = IsDefault
            ? katMotionTimeConfigService.LoadDefaultTimeConfigs()
            : katMotionTimeConfigService.LoadMotionTimeConfigs(Id);
        if (configs == null)
        {
            _configs = katMotionTimeConfigService.LoadDefaultTimeConfigs();
        }
        else
        {
            _configs = configs;
        }

        ReloadConfigs();
    }

    private void ReloadConfigs()
    {
        if (_configs.Configs.Count == 0)
        {
            _configs = katMotionTimeConfigService.LoadDefaultTimeConfigs();
        }
        ConfigViewModels.Clear();

        if (UseUnifySetting)
        {
            ConfigViewModels.Add(new MotionTimeSingleConfigViewModel(KatMotionEnum.Null,
                _configs.Configs[KatMotionEnum.TranslationXPositive]));
            return;
        }

        foreach (var config in _configs.Configs)
        {
            ConfigViewModels.Add(new MotionTimeSingleConfigViewModel(config.Key, config.Value));
        }

        ApplyTimeConfig();
    }

    [RelayCommand]
    private void ApplyTimeConfig()
    {
        _configs.Configs.Clear();
        if (UseUnifySetting)
        {
            foreach (var motion in KatMotionEnumExtensions.GetValues())
            {
                if (motion is KatMotionEnum.Null or KatMotionEnum.Stable) continue;
                var (_, config) = ConfigViewModels[0].Output();
                _configs.Configs.Add(motion, config);
            }
        }
        else
        {
            foreach (var vm in ConfigViewModels)
            {
                var (motion, config) = vm.Output();
                _configs.Configs.Add(motion, config);
            }
        }
        katActionRecognizeService.UpdateMotionTimeConfigs(_configs);
    }

    [RelayCommand]
    private void SaveTimeConfigAsync()
    {
        ApplyTimeConfig();
        _ = IsDefault
            ? katMotionTimeConfigService.SaveDefaultTimeConfig(_configs)
            : katMotionTimeConfigService.SaveTimeConfig(_configs, Id);
    }

    #endregion
}
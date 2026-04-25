using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;


namespace SpaceKatMotionMapper.ViewModels;

public partial class MotionTimeConfigViewModel(
    KatMotionRecognizeService katMotionRecognizeService,
    KatMotionTimeConfigService katMotionTimeConfigService)
    : ViewModelBase
{
    [ObservableProperty] private bool _showSingleActionModeHint;
    [ObservableProperty] private string _singleActionModeHintText = string.Empty;

    [ObservableProperty] private bool _isDefault = true;

    [ObservableProperty] private Guid _id;

    [ObservableProperty] private int _defaultLongReachTimeoutMs = 300;
    [ObservableProperty] private int _defaultSingleActionLongReachTimeoutMs = 50;
    [ObservableProperty] private double _defaultRepeatScaleFactor = 1.5;

    private KatMotionTimeConfigs _configs = new();
    private HashSet<KatMotionEnum> _singleActionMotions = [];

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

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnUseUnifySettingChanged(bool value)
    {
        ReloadConfigs();
    }

    [RelayCommand]
    private void LoadMotionTimeConfigs()
    {
        var singleActionMotions = IsDefault
            ? katMotionTimeConfigService.GetSingleActionMotionsFromDefaultConfig()
            : katMotionTimeConfigService.GetSingleActionMotionsById(Id);
        _singleActionMotions = singleActionMotions;

        var configs = IsDefault
            ? katMotionTimeConfigService.LoadDefaultTimeConfigs()
            : katMotionTimeConfigService.LoadMotionTimeConfigs(Id);
        _configs = configs ?? katMotionTimeConfigService.LoadDefaultTimeConfigs();

        DefaultLongReachTimeoutMs = _configs.DefaultLongReachTimeoutMs;
        DefaultSingleActionLongReachTimeoutMs = _configs.DefaultSingleActionLongReachTimeoutMs;
        DefaultRepeatScaleFactor = _configs.DefaultRepeatScaleFactor;

        ShowSingleActionModeHint = singleActionMotions.Count > 0;
        SingleActionModeHintText = ShowSingleActionModeHint
            ? $"提示：单动作模式已启用，{string.Join("、", singleActionMotions.Select(m => m.ToStringFast(useMetadataAttributes: true)))} 的长推触发延时固定为 {DefaultSingleActionLongReachTimeoutMs}ms。"
            : string.Empty;

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
            var previewMotion = _singleActionMotions.FirstOrDefault();
            if (previewMotion is KatMotionEnum.Null or KatMotionEnum.Stable || !_configs.Configs.ContainsKey(previewMotion))
            {
                previewMotion = KatMotionEnum.TranslationXPositive;
            }

            var isSingleAction = _singleActionMotions.Contains(previewMotion);
            var isOverridden = _configs.OverriddenMotions.Contains(previewMotion);
            ConfigViewModels.Add(new MotionTimeSingleConfigViewModel(KatMotionEnum.Null,
                _configs.Configs[previewMotion], isSingleAction, isOverridden)
            {
                OverrideChanged = OnOverrideChanged
            });
            return;
        }

        foreach (var config in _configs.Configs)
        {
            var isSingleAction = _singleActionMotions.Contains(config.Key);
            var isOverridden = _configs.OverriddenMotions.Contains(config.Key);
            ConfigViewModels.Add(new MotionTimeSingleConfigViewModel(config.Key, config.Value,
                isSingleAction, isOverridden)
            {
                OverrideChanged = OnOverrideChanged
            });
        }

        ApplyTimeConfig();
    }

    private void OnOverrideChanged(KatMotionEnum motion, bool isOverridden)
    {
        if (motion == KatMotionEnum.Null)
        {
            // 统一模式下，对所有 motion 应用同一覆盖状态
            foreach (var m in _configs.Configs.Keys)
            {
                if (isOverridden)
                    _configs.OverriddenMotions.Add(m);
                else
                    _configs.OverriddenMotions.Remove(m);
            }
        }
        else
        {
            if (isOverridden)
                _configs.OverriddenMotions.Add(motion);
            else
                _configs.OverriddenMotions.Remove(motion);
        }
    }

    [RelayCommand]
    private void ApplyTimeConfig()
    {
        _configs.DefaultLongReachTimeoutMs = DefaultLongReachTimeoutMs;
        _configs.DefaultSingleActionLongReachTimeoutMs = DefaultSingleActionLongReachTimeoutMs;
        _configs.DefaultRepeatScaleFactor = DefaultRepeatScaleFactor;

        _configs.Configs.Clear();
        if (UseUnifySetting)
        {
            foreach (var motion in KatMotionEnum.GetValues())
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
        katMotionRecognizeService.UpdateMotionTimeConfigs(_configs);
    }

    [RelayCommand]
    private void SaveTimeConfigAsync()
    {
        ApplyTimeConfig();
        _ = IsDefault
            ? katMotionTimeConfigService.SaveDefaultTimeConfig(_configs)
            : katMotionTimeConfigService.SaveTimeConfig(_configs, Id);
    }

    [RelayCommand]
    private void CopyFromDefault()
    {
        _configs.Configs.Clear();
        ReloadConfigs();
    }
    #endregion
}

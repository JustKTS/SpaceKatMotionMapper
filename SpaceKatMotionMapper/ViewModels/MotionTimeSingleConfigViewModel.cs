using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.ViewModels;

public partial class MotionTimeSingleConfigViewModel : ViewModelBase
{
    public string MotionName { get; }
    private readonly KatMotionEnum _motion;
    [ObservableProperty] private int _shortRepeatToleranceMs;
    [ObservableProperty] private int _longReachTimeoutMs;
    [ObservableProperty] private int _longReachRepeatTimeSpanMs;
    [ObservableProperty] private double _longReachRepeatScaleFactor;
    [ObservableProperty] private bool _useScaleFactor = true;

    [ObservableProperty] private bool _isSingleActionMotion;
    [ObservableProperty] private bool _isOverridden;

    public Action<KatMotionEnum, bool>? OverrideChanged { get; set; }

    private double _savedScaleFactor = 1.5;

    public bool CanEdit => !IsSingleActionMotion || IsOverridden;

    public MotionTimeSingleConfigViewModel(KatMotionEnum motion, KatTriggerTimesConfig config,
        bool isSingleActionMotion = false, bool isOverridden = false)
    {
        _motion = motion;
        IsSingleActionMotion = isSingleActionMotion;
        IsOverridden = isOverridden;

        MotionName = _motion switch
        {
            KatMotionEnum.Null => "统一",
            _ => _motion.ToStringFast(useMetadataAttributes:true)
        };
        ShortRepeatToleranceMs = config.ShortRepeatToleranceMs;
        LongReachTimeoutMs = config.LongReachTimeoutMs;
        LongReachRepeatTimeSpanMs = config.LongReachRepeatTimeSpanMs;
        LongReachRepeatScaleFactor = config.LongReachRepeatScaleFactor;
        _savedScaleFactor = config.LongReachRepeatScaleFactor > 0
            ? config.LongReachRepeatScaleFactor
            : 1.5;
        UseScaleFactor = config.LongReachRepeatScaleFactor > 0;
    }

    partial void OnIsOverriddenChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEdit));
        OverrideChanged?.Invoke(_motion, value);
    }

    partial void OnUseScaleFactorChanged(bool value)
    {
        if (!value)
        {
            _savedScaleFactor = LongReachRepeatScaleFactor > 0
                ? LongReachRepeatScaleFactor
                : _savedScaleFactor;
            LongReachRepeatScaleFactor = 0.0;
        }
        else if (_savedScaleFactor > 0)
        {
            LongReachRepeatScaleFactor = _savedScaleFactor;
        }
    }

    public (KatMotionEnum, KatTriggerTimesConfig) Output()
    {
        return (_motion, new KatTriggerTimesConfig(ShortRepeatToleranceMs, LongReachTimeoutMs, LongReachRepeatTimeSpanMs,
            LongReachRepeatScaleFactor));
    }
}

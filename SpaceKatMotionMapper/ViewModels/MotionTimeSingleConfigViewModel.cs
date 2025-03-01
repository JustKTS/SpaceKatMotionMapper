using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public MotionTimeSingleConfigViewModel(KatMotionEnum motion, KatTriggerTimesConfig config)
    {
        _motion = motion;

        MotionName = _motion switch
        {
            KatMotionEnum.Null => "统一",
            _ => _motion.ToStringFast()
        };
        ShortRepeatToleranceMs = config.ShortRepeatToleranceMs;
        LongReachTimeoutMs = config.LongReachTimeoutMs;
        LongReachRepeatTimeSpanMs = config.LongReachRepeatTimeSpanMs;
        LongReachRepeatScaleFactor = config.LongReachRepeatScaleFactor;
    }

    public (KatMotionEnum, KatTriggerTimesConfig) Output()
    {
        return (_motion, new KatTriggerTimesConfig(ShortRepeatToleranceMs, LongReachTimeoutMs, LongReachRepeatTimeSpanMs,
            UseScaleFactor ? LongReachRepeatScaleFactor : 0));
    }
}
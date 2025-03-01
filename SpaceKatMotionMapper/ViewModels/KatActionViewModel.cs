using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatActionViewModel : ObservableObject
{
    [ObservableProperty] private KatMotionEnum _katMotion;
    [ObservableProperty] private KatPressModeEnum _katPressMode;
    [ObservableProperty] private int _repeatCount = 1;
    [ObservableProperty] private KeyActionConfigViewModel _keyActionConfigGroup;
    [ObservableProperty] private int _modeNum;
    [ObservableProperty] private int _toModeNum;
    
    public KatActionsWithModeViewModel Parent { get; }

    public KatActionViewModel(KatActionsWithModeViewModel parent, int modeNum)
    {
        Parent = parent;
        KatMotion = KatMotionEnum.Null;
        KatPressMode = KatPressModeEnum.Null;
        KeyActionConfigGroup = new KeyActionConfigViewModel(this);
        ModeNum = modeNum;
        ToModeNum = ModeNum;
    }

    public KatActionConfig ToKatActionConfig()
    {
        return new KatActionConfig(new KatAction(KatMotion, KatPressMode, RepeatCount), KeyActionConfigGroup.ToKeyActionConfigList(), ModeNum, ToModeNum);
    }

    public bool LoadFromKatActionConfig(KatActionConfig actionConfig)
    {
        try
        {
            KatMotion = actionConfig.Action.Motion;
            KatPressMode = actionConfig.Action.KatPressMode;
            RepeatCount = actionConfig.Action.RepeatCount;
            ModeNum = actionConfig.ModeNum;
            ToModeNum = actionConfig.ToModeNum;
            return KeyActionConfigGroup.FromKeyActionConfig(actionConfig.ActionConfigs);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    [RelayCommand]  
    private void RemoveSelf()
    {
        var index = Parent.KatActions.IndexOf(this);
        Parent.KatActions.RemoveAt(index);
    }
}
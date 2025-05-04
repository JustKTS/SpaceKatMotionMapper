using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatMotionViewModel : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private KatMotionEnum _katMotion;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private KatPressModeEnum _katPressMode;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _repeatCount = 1;

    [ObservableProperty] private KeyActionConfigViewModel _keyActionConfigGroup;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _modeNum;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _toModeNum;
    
    // TODO:增加一个Kat长按与释放自动对应按键的长按与释放的检测
    public bool IsAvailable => CheckIsAvailable();

    // TODO：此处的重复性检查，如果两个同时错误，在一个修复后另一个不会恢复正常状态
    private bool CheckIsAvailable()
    {
        var sameCount = 0;
        foreach (var katMotionVm in Parent.KatMotions)
        {
            if (KatMotion == katMotionVm.KatMotion
                && KatPressMode == katMotionVm.KatPressMode
                && RepeatCount == katMotionVm.RepeatCount
               )
            {
                sameCount++;
            }

            if (sameCount > 1) return false;
        }

        return KatMotion is not KatMotionEnum.Null && KatPressMode is not KatPressModeEnum.Null &&
               KeyActionConfigGroup.IsAvailable;
    }


    public KatMotionsWithModeViewModel Parent { get; }

    public KatMotionViewModel(KatMotionsWithModeViewModel parent, int modeNum)
    {
        Parent = parent;
        KatMotion = KatMotionEnum.Null;
        KatPressMode = KatPressModeEnum.Null;
        KeyActionConfigGroup = new KeyActionConfigViewModel(this);
        ModeNum = modeNum;
        ToModeNum = ModeNum;
        KeyActionConfigGroup.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(KeyActionConfigViewModel.IsAvailable)) return;
            OnPropertyChanged(nameof(IsAvailable));
        };
        OnPropertyChanged(nameof(IsAvailable));
    }

    public KatMotionConfig ToKatMotionConfig()
    {
        return new KatMotionConfig(new KatMotion(KatMotion, KatPressMode, RepeatCount),
            KeyActionConfigGroup.ToKeyActionConfigList(),KeyActionConfigGroup.IsCustomDescription, KeyActionConfigGroup.KeyActionsDescription, ModeNum,
            ToModeNum);
    }

    public bool LoadFromKatMotionConfig(KatMotionConfig motionConfig)
    {
        try
        {
            KatMotion = motionConfig.Motion.Motion;
            KatPressMode = motionConfig.Motion.KatPressMode;
            RepeatCount = motionConfig.Motion.RepeatCount;
            ModeNum = motionConfig.ModeNum;
            ToModeNum = motionConfig.ToModeNum;
            OnPropertyChanged(nameof(IsAvailable));
            if (motionConfig.IsCustomDescription)
            {
                KeyActionConfigGroup.IsCustomDescription = true;
                KeyActionConfigGroup.KeyActionsDescription = motionConfig.KeyActionsDescription;
            }

            return KeyActionConfigGroup.FromKeyActionConfig(motionConfig.ActionConfigs);
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
        var index = Parent.KatMotions.IndexOf(this);
        Parent.RemoveKatMotionConfigCommand.Execute(index);
    }
}
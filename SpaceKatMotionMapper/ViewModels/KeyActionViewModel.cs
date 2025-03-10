using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageExt;
using LanguageExt.ClassInstances.Const;
using SpaceKatMotionMapper.Models;
using WindowsInput;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KeyActionViewModel : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))] [NotifyPropertyChangedFor(nameof(IsDelay))]
    private ActionType _actionType;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private string _key;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private PressModeEnum _pressMode;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _multiplier;

    public bool IsDelay => ActionType == ActionType.Delay;

    public bool IsAvailable
    {
        get
        {
            switch (ActionType)
            {
                case ActionType.KeyBoard:
                    return Key != "None" && PressMode != PressModeEnum.None;
                case ActionType.Mouse:
                    if (Key == "None") return false;
                    try
                    {
                        var key = MouseButtonEnumExtensions.Parse(Key, ignoreCase: true,
                            allowMatchingMetadataAttribute: true);
                        return key switch
                        {
                            MouseButtonEnum.ScrollUp or MouseButtonEnum.ScrollDown =>
                                Multiplier != 0,
                            _ => PressMode != PressModeEnum.None
                        };
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return false;
                    }
                case ActionType.Delay:
                    return Multiplier >= 15;
                default:
                    return false;
            }
        }
    }


    private readonly KeyActionConfigViewModel _parent;
    
    public KeyActionViewModel(KeyActionConfigViewModel parent, ActionType actionType, string key,
        PressModeEnum pressMode, int multiplier)
    {
        _parent = parent;
        ActionType = actionType;
        Key = key;
        PressMode = pressMode;
        Multiplier = multiplier;
    }

    public KeyActionViewModel(KeyActionConfigViewModel parent) : this(parent, ActionType.KeyBoard,
        VirtualKeyCode.None.ToString(), PressModeEnum.None, 1)
    {
    }


    [RelayCommand]
    private void Remove()
    {
        var index = _parent.ActionConfigGroups.IndexOf(this);
        _parent.RemoveActionConfigCommand.Execute(index);
    }

    [RelayCommand]
    private void InsertNextAction()
    {
        var index = _parent.ActionConfigGroups.IndexOf(this);
        _parent.InsertNextActionConfig(index);
    }
    
    [RelayCommand]
    private void InsertNextDelay()
    {
        var index = _parent.ActionConfigGroups.IndexOf(this);
        _parent.InsertNextDelayConfig(index);
    }

    public KeyActionConfig ToKeyActionConfig()
    {
        return new KeyActionConfig(ActionType, Key, PressMode, Multiplier);
    }

    public bool FromKeyActionConfig(KeyActionConfig config)
    {
        ActionType = config.ActionType;
        Key = config.Key;
        PressMode = config.PressMode;
        Multiplier = config.Multiplier;
        return true;
    }
}
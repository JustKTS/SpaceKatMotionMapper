using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Models;
using WindowsInput;

namespace ProgramSpecificConfigCreator.ViewModels;

public partial class KeyActionViewModel : ViewModelBase
{
    #region 属性定义

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))] [NotifyPropertyChangedFor(nameof(IsDelay))]
    private ActionType _actionType;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private string _key;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private PressModeEnum _pressMode;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _multiplier;


    private readonly KeyActionConfigViewModel _parent;

    public bool IsDelay => ActionType == ActionType.Delay;

    #endregion

    #region 是否可用

    public bool IsAvailable => CheckIsAvailable();

    private bool CheckIsAvailable()
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

    #endregion

    public KeyActionViewModel(KeyActionConfigViewModel parent, ActionType actionType, string key,
        PressModeEnum pressMode, int multiplier)
    {
        _parent = parent;
        ActionType = actionType;
        Key = key;
        PressMode = pressMode;
        Multiplier = multiplier;
        OnPropertyChanged(nameof(IsAvailable));
    }

    public KeyActionViewModel(KeyActionConfigViewModel parent) : this(parent, ActionType.KeyBoard,
        VirtualKeyCode.None.ToString(), PressModeEnum.None, 1)
    {
        OnPropertyChanged(nameof(IsAvailable));
    }

    # region 添加删除

    [RelayCommand]
    private void Remove()
    {
        var index = _parent.ActionConfigGroups.IndexOf(this);
        _parent.RemoveActionConfigCommand.Execute(index);
        OnPropertyChanged(nameof(IsAvailable));

    }

    [RelayCommand]
    private void InsertNextAction()
    {
        var index = _parent.ActionConfigGroups.IndexOf(this);
        _parent.InsertNextActionConfig(index);
        OnPropertyChanged(nameof(IsAvailable));
    }

    [RelayCommand]
    private void InsertNextDelay()
    {
        var index = _parent.ActionConfigGroups.IndexOf(this);
        _parent.InsertNextDelayConfig(index);
        OnPropertyChanged(nameof(IsAvailable));
    }

    #endregion

    #region 转换为配置

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

    #endregion
}
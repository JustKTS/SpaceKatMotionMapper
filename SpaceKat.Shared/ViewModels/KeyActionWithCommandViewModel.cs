using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKat.Shared.ViewModels;

public partial class KeyActionWithCommandViewModel : ObservableObject
{
    private readonly IKeyActionPressModePolicy _pressModePolicy;
    private readonly IKeyActionAvailabilityValidator _availabilityValidator;
    private readonly KeyActionAvailabilityValidationOptions _availabilityOptions;

    #region 属性定义

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))] [NotifyPropertyChangedFor(nameof(IsDelay))]
    private ActionType _actionType;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private string _key;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private PressModeEnum _pressMode;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _multiplier;
    
    public bool IsDelay => ActionType == ActionType.Delay;

    partial void OnActionTypeChanged(ActionType value)
    {
        Key = KeyActionConstants.NoneKeyValue;
        PressMode = _pressModePolicy.GetDefaultPressMode(value);

        if (value == ActionType.Delay)
        {
            if (Multiplier < KeyActionConstants.MinDelayMultiplier)
            {
                Multiplier = KeyActionConstants.MinDelayMultiplier;
            }

            return;
        }

        if (Multiplier == 0)
        {
            Multiplier = 1;
        }
    }

    #endregion

    #region 是否可用

    public bool IsAvailable => CheckIsAvailable();

    private bool CheckIsAvailable()
    {
        return _availabilityValidator.Validate(ActionType, Key, PressMode, Multiplier, _availabilityOptions);
    }

    #endregion
    
    public KeyActionWithCommandViewModel(ActionType actionType, string key,
        PressModeEnum pressMode, int multiplier,
        IKeyActionPressModePolicy? pressModePolicy = null,
        IKeyActionAvailabilityValidator? availabilityValidator = null,
        KeyActionAvailabilityValidationOptions? availabilityOptions = null)
    {
        _pressModePolicy = pressModePolicy ?? new DefaultKeyActionPressModePolicy();
        _availabilityValidator = availabilityValidator ?? new KeyActionAvailabilityValidator();
        _availabilityOptions = availabilityOptions ?? new KeyActionAvailabilityValidationOptions(RequirePositiveScrollMultiplier: false);
        ActionType = actionType;
        Key = key;
        PressMode = _pressModePolicy.CoercePressMode(actionType, pressMode);
        Multiplier = multiplier;
        OnPropertyChanged(nameof(IsAvailable));
    }

    # region 添加删除

    public ICommand? RemoveActionConfigCommand { get; set; }
    public ICommand? InsertNextActionConfigCommand { get; set; }
    public ICommand? InsertNextDelayConfigCommand { get; set; }
    

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
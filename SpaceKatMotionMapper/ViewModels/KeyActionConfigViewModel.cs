using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.ViewModels.PressModePolicies;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KeyActionConfigViewModel : ViewModelBase
{
# if DEBUG
    public KeyActionConfigViewModel() : this(null!)
    {
    }
#endif

    [ObservableProperty] private bool _isCustomDescription;
    [ObservableProperty] private string _keyActionsDescription = string.Empty;

    public ObservableCollection<int> CurrentConfigModeNums { get; }

    public ObservableCollection<KeyActionWithCommandViewModel> ActionConfigGroups { get; set; }

    public static IReadOnlyList<string> KeyNames => VirtualKeyHelpers.KeyNames;
    public KatMotionViewModel Parent { get; }

    public bool IsAvailable => CheckIsAvailable();

    public bool IsSingleActionMode => Parent.IsSingleActionMode;

    private bool CheckIsAvailable()
    {
        if (!ActionConfigGroups.All(e => e.IsAvailable)) return false;

        var actions = ToKeyActionConfigList();
        return _semanticValidators.All(validator => validator.Validate(actions, Parent.IsSingleActionMode));
    }

    public KeyActionConfigViewModel(
        KatMotionViewModel parent,
        MetaKeyPresetService? metaKeyPresetService = null,
        PopUpNotificationService? popUpNotificationService = null,
        IHotKeyActionExpansionService? hotKeyActionExpansionService = null,
        IKeyActionPressModePolicy? pressModePolicy = null,
        IKeyActionAvailabilityValidator? keyActionAvailabilityValidator = null,
        IEnumerable<IKeyActionConfigSemanticValidator>? semanticValidators = null,
        IKeyActionConfigStrategyProfile? strategyProfile = null)
    {
        Parent = parent;
        _strategyProfile = strategyProfile ?? new DefaultKeyActionConfigStrategyProfile();
        _hotKeyActionExpansionService = hotKeyActionExpansionService ?? _strategyProfile.HotKeyActionExpansionService;
        _pressModePolicyOverride = pressModePolicy;
        _keyActionAvailabilityValidator = keyActionAvailabilityValidator ?? _strategyProfile.AvailabilityValidator;
        _semanticValidators = semanticValidators?.ToArray() ?? _strategyProfile.SemanticValidators;

        // 尝试从服务定位器获取服务，如果失败（测试环境）则使用传入的服务或 null
        try
        {
            _metaKeyPresetService = metaKeyPresetService ?? App.GetRequiredService<MetaKeyPresetService>();
            _popUpNotificationService = popUpNotificationService ?? App.GetRequiredService<PopUpNotificationService>();
        }
        catch (NullReferenceException)
        {
            // 在测试环境中，App.Current 可能为 null，这是可以接受的
            _metaKeyPresetService = metaKeyPresetService;
            _popUpNotificationService = popUpNotificationService;
        }

        CurrentConfigModeNums = Parent.Parent.Parent.Parent.KatMotionsModeNums;
        ActionConfigGroups = [];

        ActionConfigGroups.CollectionChanged += (_, e) =>
        {
            // 处理新增项：订阅PropertyChanged
            if (e.NewItems != null)
            {
                foreach (KeyActionWithCommandViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }

            // 处理移除项：取消订阅避免内存泄漏
            if (e.OldItems == null) return;

            foreach (KeyActionWithCommandViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        };

        AddActionConfig();
    }

    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(KeyActionWithCommandViewModel.IsAvailable))
        {
            OnPropertyChanged(nameof(IsAvailable));
        }
    }

    # region 添加删除

    [RelayCommand]
    private void AddActionConfig()
    {
        ActionConfigGroups.Add(CreateActionViewModel());
        OnPropertyChanged(nameof(IsAvailable));
    }

    private KeyActionWithCommandViewModel CreateActionViewModel(
        ActionType actionType = ActionType.KeyBoard,
        string key = KeyActionConstants.NoneKeyValue,
        PressModeEnum pressMode = PressModeEnum.None,
        int multiplier = 1)
    {
        var item = new KeyActionWithCommandViewModel(actionType, key, pressMode, multiplier,
            new DelegatingPressModePolicy(ResolvePressModePolicy), _keyActionAvailabilityValidator,
            MainProjectAvailabilityOptions);
        item.RemoveActionConfigCommand = new RelayCommand(() =>
        {
            var index = ActionConfigGroups.IndexOf(item);
            RemoveActionConfig(index);
        });
        item.InsertNextActionConfigCommand = new RelayCommand(() =>
        {
            var index = ActionConfigGroups.IndexOf(item);
            InsertNextActionConfig(index);
        });
        item.InsertNextDelayConfigCommand = new RelayCommand(() =>
        {
            var index = ActionConfigGroups.IndexOf(item);
            InsertNextDelayConfig(index);
        });
        return item;
    }

    private static readonly KeyActionAvailabilityValidationOptions MainProjectAvailabilityOptions =
        new(RequirePositiveScrollMultiplier: true);

    private IKeyActionPressModePolicy ResolvePressModePolicy()
    {
        if (_pressModePolicyOverride != null) return _pressModePolicyOverride;
        return _strategyProfile.GetPressModePolicy(Parent.IsSingleActionMode);
    }

    [RelayCommand]
    private void RemoveActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        var removed = ActionConfigGroups[index];
        removed.RemoveActionConfigCommand = null;
        removed.InsertNextActionConfigCommand = null;
        removed.InsertNextDelayConfigCommand = null;
        ActionConfigGroups.RemoveAt(index);
        if (ActionConfigGroups.Count == 0)
        {
            ActionConfigGroups.Add(CreateActionViewModel(
                ActionType.KeyBoard,
                KeyActionConstants.NoneKeyValue,
                PressModeEnum.None,
                1));
        }

        OnPropertyChanged(nameof(IsAvailable));
    }

    public void InsertNextActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index + 1, CreateActionViewModel(ActionType.KeyBoard,
            KeyActionConstants.NoneKeyValue,
            PressModeEnum.None, 1));
        OnPropertyChanged(nameof(IsAvailable));
    }

    public void InsertNextDelayConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index + 1, CreateActionViewModel(ActionType.Delay,
            KeyActionConstants.NoneKeyValue,
            PressModeEnum.None, KeyActionConstants.MinDelayMultiplier));
        OnPropertyChanged(nameof(IsAvailable));
    }

    #endregion

    # region 读写

    public List<KeyActionConfig> ToKeyActionConfigList()
    {
        return ToKeyActionConfigListCore();
    }

    private List<KeyActionConfig> ToKeyActionConfigListCore()
    {
        return ActionConfigGroups.Select(actionGroup => actionGroup.ToKeyActionConfig()).ToList();
    }

    public bool FromKeyActionConfig(IEnumerable<KeyActionConfig> keyActionConfigs)
    {
        try
        {
            var actionConfigs = keyActionConfigs as List<KeyActionConfig> ?? keyActionConfigs.ToList();
            return FromKeyActionConfigCore(actionConfigs);
        }
        catch (Exception e)
        {
            Log.Error(e, "[{ViewModel}] Failed to load key action config", nameof(KeyActionConfigViewModel));
            return false;
        }
    }

    private bool FromKeyActionConfigCore(IReadOnlyList<KeyActionConfig> actionConfigs)
    {
        // 手动取消订阅所有项，避免 Clear() 时的内存泄漏
        foreach (var item in ActionConfigGroups)
        {
            item.PropertyChanged -= ChildPropertyChanged;
            item.RemoveActionConfigCommand = null;
            item.InsertNextActionConfigCommand = null;
            item.InsertNextDelayConfigCommand = null;
        }

        ActionConfigGroups.Clear();
        foreach (var keyActionConfig in actionConfigs)
        {
            var actionConfigGroup = CreateActionViewModel();
            var ret = actionConfigGroup.FromKeyActionConfig(keyActionConfig);
            if (!ret) return false;
            ActionConfigGroups.Add(actionConfigGroup);
        }

        OnPropertyChanged(nameof(IsAvailable));
        return true;
    }

    #endregion

    # region HotKey转动作组

    public void AddHotKeyActions(bool useCtrl,
        bool useWin,
        bool useAlt,
        bool useShift,
        KeyCodeWrapper hotKey,
        string? customDescription = null)
    {
        if (customDescription is not null)
        {
            KeyActionsDescription = customDescription;
            IsCustomDescription = true;
        }

        var combinationKeys = new CombinationKeysRecord(useCtrl, useShift, useAlt, useWin, hotKey);
        var expandedActions = _hotKeyActionExpansionService.Expand(combinationKeys, Parent.IsSingleActionMode);
        _ = FromKeyActionConfig(expandedActions);
    }

    #endregion

    #region 添加自定义动作组

    private void AddCustomActions(string description, IEnumerable<KeyActionConfig> keyActionConfigs)
    {
        KeyActionsDescription = description;
        IsCustomDescription = true;
        FromKeyActionConfig(keyActionConfigs);
    }

    [RelayCommand]
    private async Task OpenPresetSelector()
    {
        await OverlayDialog
            .ShowCustomAsync<MetaKeyPresetSelectorView, MetaKeyPresetSelectorViewModel, object?>(
                new MetaKeyPresetSelectorViewModel(this, new RelayCommand<KeyActionsForPresetRecord>(
                        param =>
                        {
                            if (param is null) return;
                            AddCustomActions(param.Description, param.Actions);
                        }),
                    new RelayCommand<KeyValuePair<string, CombinationKeysRecord>>(param =>
                    {
                        AddHotKeyActions(param.Value.UseCtrl, param.Value.UseWin, param.Value.UseAlt,
                            param.Value.UseShift, param.Value.Key, param.Key);
                    })), KatMotionGroupConfigWindow.LocalHost,
                new OverlayDialogOptions
                {
                    HorizontalAnchor = HorizontalPosition.Center,
                    VerticalAnchor = VerticalPosition.Center,
                    Buttons = DialogButton.None,
                    CanDragMove = true,
                    CanLightDismiss = true,
                    CanResize = true,
                    FullScreen = false,
                    IsCloseButtonVisible = false,
                    Mode = DialogMode.None
                }
            );
    }

    #endregion

    #region 添加动作组到收藏

    private readonly MetaKeyPresetService? _metaKeyPresetService;
    private readonly PopUpNotificationService? _popUpNotificationService;
    private readonly IHotKeyActionExpansionService _hotKeyActionExpansionService;
    private readonly IKeyActionConfigStrategyProfile _strategyProfile;
    private readonly IKeyActionPressModePolicy? _pressModePolicyOverride;
    private readonly IKeyActionAvailabilityValidator _keyActionAvailabilityValidator;
    private readonly IReadOnlyList<IKeyActionConfigSemanticValidator> _semanticValidators;

    [RelayCommand]
    private void AddToFavPreset()
    {
        if (_metaKeyPresetService == null) return; // 在测试环境中可能为 null

        var ret = _metaKeyPresetService.AddToFavPreset(KeyActionsDescription, ToKeyActionConfigList());
        _ = ret.Match(s =>
        {
            if (!s) return s;
            _popUpNotificationService?.PopInKatMotionConfigWindow(Parent.Parent.Parent.Parent.Id, NotificationType.Success,
                    $"收藏\"{KeyActionsDescription}\"成功");
            return s;
        }, ex =>
        {
            _popUpNotificationService?.PopInKatMotionConfigWindow(Parent.Parent.Parent.Parent.Id, NotificationType.Error, ex.Message);
            return false;
        });
    }

    #endregion
}
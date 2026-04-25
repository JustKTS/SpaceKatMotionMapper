using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.ViewModels;

public partial class KeyActionConfigV2ViewModel : KeyActionConfigEditableBaseViewModel
{
    private readonly IHotKeyActionExpansionService _hotKeyActionExpansionService;

    [ObservableProperty] private bool _isCustomDescription;
    [ObservableProperty] private string _keyActionsDescription = string.Empty;

    public static IReadOnlyList<string> KeyNames => VirtualKeyHelpers.KeyNames;

    public KeyActionConfigV2ViewModel(
        IHotKeyActionExpansionService? hotKeyActionExpansionService = null,
        ISharedKeyActionConfigStrategyProfile? strategyProfile = null) : base(strategyProfile)
    {
        _hotKeyActionExpansionService = hotKeyActionExpansionService ?? StrategyProfile.HotKeyActionExpansionService;
        
        CombinationKeysWithCommandVM.OnKeysSetted += (_, e) =>
        {
            AddHotKeyActions(e.UseCtrl, e.UseWin, e.UseAlt, e.UseShift, e.Key);
        };
    }

    # region 读写

    public List<KeyActionConfig> ToKeyActionConfigList()
    {
        return ToKeyActionConfigListCore();
    }

    public bool FromKeyActionConfig(IEnumerable<KeyActionConfig> keyActionConfigs)
    {
        try
        {
            var actionConfigs = keyActionConfigs as List<KeyActionConfig> ?? keyActionConfigs.ToList();
            return FromKeyActionConfigCore(actionConfigs, loadedConfigs =>
            {
                var loadedConfigList = loadedConfigs as List<KeyActionConfig> ?? loadedConfigs.ToList();
                var combinationKeys = CombinationKeysHelper.ValidateIsCombinationKeys(loadedConfigList)
                    ? CombinationKeysHelper.ConvertActionsToCombinationRecord(loadedConfigList)
                    : null;
                if (combinationKeys != null)
                {
                    CombinationKeysWithCommandVM.FromRecord(combinationKeys);
                }

                return true;
            });
        }
        catch (Exception e)
        {
            Log.Error(e, "[{ViewModel}] Failed to load key action config", nameof(KeyActionConfigV2ViewModel));
            return false;
        }
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
        var expandedActions = _hotKeyActionExpansionService.Expand(combinationKeys, isSingleActionMode: false);
        List<KeyActionWithCommandViewModel> list = expandedActions
            .Select(action => CreateManagedActionConfigVm(action.ActionType, action.Key, action.PressMode, action.Multiplier))
            .ToList();

        ActionConfigGroups.Clear();
        list.Iter(ActionConfigGroups.Add);
        OnPropertyChanged(nameof(IsAvailable));
    }

    #endregion
    
    #region 组合键
    public CombinationKeysWithCommandVM CombinationKeysWithCommandVM { get; set; } = new();
    #endregion

    //
    // #region 添加自定义动作组
    //
    // public void AddCustomActions(string description, IEnumerable<KeyActionConfig> keyActionConfigs)
    // {
    //     KeyActionsDescription = description;
    //     IsCustomDescription = true;
    //     FromKeyActionConfig(keyActionConfigs);
    // }
    //
    // [RelayCommand]
    // private async Task OpenPresetSelector()
    // {
    //     await OverlayDialog
    //         .ShowCustomModal<MetaKeyPresetSelectorView, MetaKeyPresetSelectorViewModel, object?>(
    //             new MetaKeyPresetSelectorViewModel(this, new RelayCommand<KeyActionsForPresetRecord>(
    //                     param =>
    //                     {
    //                         if (param is null) return;
    //                         AddCustomActions(param.Description, param.Actions);
    //                     }),
    //                 new RelayCommand<KeyValuePair<string, CombinationKeysRecord>>(param =>
    //                 {
    //                     AddHotKeyActions(param.Value.UseCtrl, param.Value.UseWin, param.Value.UseAlt,
    //                         param.Value.UseShift, param.Value.Key, param.Key);
    //                 })), KatMotionGroupConfigWindow.LocalHost,
    //             new OverlayDialogOptions
    //             {
    //                 HorizontalAnchor = HorizontalPosition.Center,
    //                 VerticalAnchor = VerticalPosition.Center,
    //                 Buttons = DialogButton.None,
    //                 CanDragMove = true,
    //                 CanLightDismiss = true,
    //                 CanResize = true,
    //                 FullScreen = false,
    //                 IsCloseButtonVisible = false,
    //                 Mode = DialogMode.None
    //             }
    //         );
    // }
    //
    // #endregion
    //
    // #region 添加动作组到收藏
    //
    // private readonly MetaKeyPresetService _metaKeyPresetService =
    //     App.GetRequiredService<MetaKeyPresetService>();
    //
    // [RelayCommand]
    // private void AddToFavPreset()
    // {
    //     var ret = _metaKeyPresetService.AddToFavPreset(KeyActionsDescription, ToKeyActionConfigList());
    //     _ = ret.Match(s =>
    //     {
    //         if (!s) return s;
    //         App.GetRequiredService<PopUpNotificationService>()
    //             .PopInKatMotionConfigWindow(Parent.Parent.Parent.Id, NotificationType.Success,
    //                 $"收藏\"{KeyActionsDescription}\"成功");
    //         return s;
    //     }, ex =>
    //     {
    //         App.GetRequiredService<PopUpNotificationService>()
    //             .PopInKatMotionConfigWindow(Parent.Parent.Parent.Id, NotificationType.Error, ex.Message);
    //         return false;
    //     });
    // }
    //
    // #endregion
}

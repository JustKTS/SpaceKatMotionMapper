using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using WindowsInput;

namespace SpaceKat.Shared.ViewModels;

public partial class KeyActionConfigV2ViewModel : ObservableObject
{
    [ObservableProperty] private bool _isCustomDescription;
    [ObservableProperty] private string _keyActionsDescription = string.Empty;

    public ObservableCollection<KeyActionWithCommandViewModel> ActionConfigGroups { get; set; }

    public static IReadOnlyList<string> KeyNames => VirtualKeyHelpers.KeyNames;
    public bool IsAvailable => CheckIsAvailable();

    private bool CheckIsAvailable()
    {
        return ActionConfigGroups.All(e => e.IsAvailable);
    }

    public KeyActionConfigV2ViewModel()
    {
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
        
        CombinationKeysWithCommandVM.OnKeysSetted += (_, e) =>
        {
            AddHotKeyActions(e.UseCtrl, e.UseWin, e.UseAlt, e.UseShift, e.Key);
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
        ActionConfigGroups.Add(CreateKeyActionVm());
        OnPropertyChanged(nameof(IsAvailable));
    }

    private KeyActionWithCommandViewModel CreateKeyActionVm(ActionType actionType = ActionType.KeyBoard,
        string key = nameof(VirtualKeyCode.None),
        PressModeEnum pressMode = PressModeEnum.None,
        int multiplier = 1)
    {
        var item = new KeyActionWithCommandViewModel(actionType, key, pressMode, multiplier);
        item.RemoveActionConfigCommand = new RelayCommand(() => RemoveActionConfig(item));
        item.InsertNextActionConfigCommand = new RelayCommand(() => InsertNextActionConfig(item));
        item.InsertNextDelayConfigCommand = new RelayCommand(() => InsertNextDelayConfig(item));
        return item;
    }

    private void RemoveActionConfig(KeyActionWithCommandViewModel vm)
    {
        var index = ActionConfigGroups.IndexOf(vm);
        ActionConfigGroups.RemoveAt(index);

        vm.RemoveActionConfigCommand = null;
        vm.InsertNextActionConfigCommand = null;
        vm.InsertNextDelayConfigCommand = null;

        if (ActionConfigGroups.Count == 0)
        {
            AddActionConfigCommand.Execute(null);
        }

        OnPropertyChanged(nameof(IsAvailable));
    }

    private void InsertNextActionConfig(KeyActionWithCommandViewModel vm)
    {
        var index = ActionConfigGroups.IndexOf(vm);
        ActionConfigGroups.Insert(index + 1, CreateKeyActionVm());
        OnPropertyChanged(nameof(IsAvailable));
    }

    private void InsertNextDelayConfig(KeyActionWithCommandViewModel vm)
    {
        var index = ActionConfigGroups.IndexOf(vm);

        ActionConfigGroups.Insert(index + 1, CreateKeyActionVm(ActionType.Delay,
            nameof(VirtualKeyCode.None),
            PressModeEnum.None, 15));
        OnPropertyChanged(nameof(IsAvailable));
    }

    #endregion

    # region 读写

    public List<KeyActionConfig> ToKeyActionConfigList()
    {
        return ActionConfigGroups.Select(actionGroup => actionGroup.ToKeyActionConfig()).ToList();
    }

    public bool FromKeyActionConfig(IEnumerable<KeyActionConfig> keyActionConfigs)
    {
        try
        {
            ActionConfigGroups.Clear();
            var actionConfigs = keyActionConfigs as List<KeyActionConfig> ?? keyActionConfigs.ToList();
            foreach (var keyActionConfig in actionConfigs)
            {
                var actionConfigGroup = CreateKeyActionVm();
                var ret = actionConfigGroup.FromKeyActionConfig(keyActionConfig);
                if (!ret) return false;
                ActionConfigGroups.Add(actionConfigGroup);
            }
            
            var combinationKeys = CombinationKeysHelper.ValidateIsCombinationKeys(actionConfigs)?CombinationKeysHelper.ConvertActionsToCombinationRecord(actionConfigs):null;
            if (combinationKeys != null)
            {
                CombinationKeysWithCommandVM.FromRecord(combinationKeys);
            }
            
            OnPropertyChanged(nameof(IsAvailable));
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    #endregion

    # region HotKey转动作组

    public void AddHotKeyActions(bool useCtrl,
        bool useWin,
        bool useAlt,
        bool useShift,
        VirtualKeyCode hotKey,
        string? customDescription = null)
    {
        if (customDescription is not null)
        {
            KeyActionsDescription = customDescription;
            IsCustomDescription = true;
        }

        List<KeyActionWithCommandViewModel> list = [];
        if (useCtrl)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard,
                VirtualKeyCode.CONTROL.GetWrappedName(), PressModeEnum.Press, 1));
        if (useWin)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard, VirtualKeyCode.LWIN.GetWrappedName(),
                PressModeEnum.Press, 1));
        if (useAlt)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard, VirtualKeyCode.MENU.GetWrappedName(),
                PressModeEnum.Press, 1));
        if (useShift)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard, VirtualKeyCode.SHIFT.GetWrappedName(),
                PressModeEnum.Press, 1));
        list.Add(CreateKeyActionVm(ActionType.KeyBoard, hotKey.GetWrappedName(),
            PressModeEnum.Click,
            1));
        if (useShift)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard, VirtualKeyCode.SHIFT.GetWrappedName(),
                PressModeEnum.Release, 1));
        if (useAlt)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard, VirtualKeyCode.MENU.GetWrappedName(),
                PressModeEnum.Release, 1));
        if (useWin)
            list.Add(CreateKeyActionVm(ActionType.KeyBoard, VirtualKeyCode.LWIN.GetWrappedName(),
                PressModeEnum.Release, 1));
        if (useCtrl)
            list.Add(CreateKeyActionVm( ActionType.KeyBoard,
                VirtualKeyCode.CONTROL.GetWrappedName(), PressModeEnum.Release, 1));
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;
using WindowsInput;

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

    public ObservableCollection<KeyActionViewModel> ActionConfigGroups { get; set; }

    public static IReadOnlyList<string> KeyNames => VirtualKeyHelpers.KeyNames;
    public KatMotionViewModel Parent { get; }

    public bool IsAvailable => CheckIsAvailable();

    private bool CheckIsAvailable()
    {
        return ActionConfigGroups.All(e => e.IsAvailable);
    }

    public KeyActionConfigViewModel(KatMotionViewModel parent)
    {
        Parent = parent;

        CurrentConfigModeNums = Parent.Parent.Parent.KatMotionsModeNums;
        ActionConfigGroups = [];

        ActionConfigGroups.CollectionChanged += (sender, e) =>
        {
            // 处理新增项：订阅PropertyChanged
            if (e.NewItems != null)
            {
                foreach (KeyActionViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }

            // 处理移除项：取消订阅避免内存泄漏
            if (e.OldItems == null) return;

            foreach (KeyActionViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        };

        AddActionConfig();
    }

    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(KeyActionViewModel.IsAvailable))
        {
            OnPropertyChanged(nameof(IsAvailable));
        }
    }

    # region 添加删除

    [RelayCommand]
    private void AddActionConfig()
    {
        ActionConfigGroups.Add(new KeyActionViewModel(this));
        OnPropertyChanged(nameof(IsAvailable));
    }

    [RelayCommand]
    private void RemoveActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.RemoveAt(index);
        if (ActionConfigGroups.Count == 0)
        {
            ActionConfigGroups.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.None.ToString(),
                PressModeEnum.None, 1));
        }

        OnPropertyChanged(nameof(IsAvailable));
    }

    public void InsertNextActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index + 1, new KeyActionViewModel(this, ActionType.KeyBoard,
            VirtualKeyCode.None.ToString(),
            PressModeEnum.None, 1));
        OnPropertyChanged(nameof(IsAvailable));
    }

    public void InsertNextDelayConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index + 1, new KeyActionViewModel(this, ActionType.Delay,
            VirtualKeyCode.None.ToString(),
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
            foreach (var keyActionConfig in keyActionConfigs)
            {
                var actionConfigGroup = new KeyActionViewModel(this);
                var ret = actionConfigGroup.FromKeyActionConfig(keyActionConfig);
                if (!ret) return false;
                ActionConfigGroups.Add(actionConfigGroup);
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

        List<KeyActionViewModel> list = [];
        if (useCtrl)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard,
                VirtualKeyCode.CONTROL.GetWrappedName(), PressModeEnum.Press, 1));
        if (useWin)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.LWIN.GetWrappedName(),
                PressModeEnum.Press, 1));
        if (useAlt)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.MENU.GetWrappedName(),
                PressModeEnum.Press, 1));
        if (useShift)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.SHIFT.GetWrappedName(),
                PressModeEnum.Press, 1));
        list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, hotKey.GetWrappedName(),
            PressModeEnum.Click,
            1));
        if (useShift)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.SHIFT.GetWrappedName(),
                PressModeEnum.Release, 1));
        if (useAlt)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.MENU.GetWrappedName(),
                PressModeEnum.Release, 1));
        if (useWin)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.LWIN.GetWrappedName(),
                PressModeEnum.Release, 1));
        if (useCtrl)
            list.Add(new KeyActionViewModel(this, ActionType.KeyBoard,
                VirtualKeyCode.CONTROL.GetWrappedName(), PressModeEnum.Release, 1));
        ActionConfigGroups.Clear();
        list.Iter(ActionConfigGroups.Add);
        OnPropertyChanged(nameof(IsAvailable));
    }

    #endregion

    #region 添加自定义动作组

    public void AddCustomActions(string description, IEnumerable<KeyActionConfig> keyActionConfigs)
    {
        KeyActionsDescription = description;
        IsCustomDescription = true;
        FromKeyActionConfig(keyActionConfigs);
    }

    [RelayCommand]
    private async Task OpenPresetSelector()
    {
        await OverlayDialog
            .ShowCustomModal<MetaKeyPresetSelectorView, MetaKeyPresetSelectorViewModel, object?>(
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

    private readonly MetaKeyPresetService _metaKeyPresetService =
        App.GetRequiredService<MetaKeyPresetService>();

    [RelayCommand]
    private void AddToFavPreset()
    {
        var ret = _metaKeyPresetService.AddToFavPreset(KeyActionsDescription, ToKeyActionConfigList());
        _ = ret.Match(s =>
        {
            if (!s) return s;
            App.GetRequiredService<PopUpNotificationService>()
                .PopInKatMotionConfigWindow(Parent.Parent.Parent.Id, NotificationType.Success,
                    $"收藏\"{KeyActionsDescription}\"成功");
            return s;
        }, ex =>
        {
            App.GetRequiredService<PopUpNotificationService>()
                .PopInKatMotionConfigWindow(Parent.Parent.Parent.Id, NotificationType.Error, ex.Message);
            return false;
        });
    }

    #endregion
}
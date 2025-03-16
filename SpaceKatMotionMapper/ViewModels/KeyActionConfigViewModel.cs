using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Metsys.Bson;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
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

    public ObservableCollection<int> CurrentConfigModeNums { get; }

    public ObservableCollection<KeyActionViewModel> ActionConfigGroups { get; set; }

    public IReadOnlyList<string> KeyNames => VirtualKeyHelper.KeyNames;
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

    public HotKeyCodeEnum[] HotKeyEnums { get; set; } = HotKeyCodeEnumExtensions.GetValues();
    
    public void AddHotKeyActions(bool useCtrl,
        bool useWin,
        bool useAlt,
        bool useShift,
        VirtualKeyCode hotKey)
    {
        List<KeyActionViewModel> list = [];
        if (useCtrl) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.CONTROL.ToWarpCodeName(), PressModeEnum.Press, 1));
        if (useWin) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.LWIN.ToWarpCodeName(), PressModeEnum.Press, 1));
        if (useAlt) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.MENU.ToWarpCodeName(), PressModeEnum.Press, 1));
        if (useShift) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.SHIFT.ToWarpCodeName(), PressModeEnum.Press, 1));
        list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, hotKey.ToWarpCodeName(), PressModeEnum.Click,
            1));
        if (useShift) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.SHIFT.ToWarpCodeName(), PressModeEnum.Release, 1));
        if (useAlt) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.MENU.ToWarpCodeName(), PressModeEnum.Release, 1));
        if (useWin) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.LWIN.ToWarpCodeName(), PressModeEnum.Release, 1));
        if (useCtrl) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.CONTROL.ToWarpCodeName(), PressModeEnum.Release, 1));
        ActionConfigGroups.Clear();
        list.Iter(ActionConfigGroups.Add);
        OnPropertyChanged(nameof(IsAvailable));
    }

    #endregion
}
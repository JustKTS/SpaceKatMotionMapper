using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ABI.Windows.Perception.People;
using CommunityToolkit.Mvvm.Input;
using SpaceKatMotionMapper.Models;
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

    public ObservableCollection<int> CurrentConfigModeNums { get; }

    public ObservableCollection<KeyActionViewModel> ActionConfigGroups { get; set; }

    public KatActionViewModel Parent { get; }

    public KeyActionConfigViewModel(KatActionViewModel parent)
    {
        Parent = parent;

        CurrentConfigModeNums = Parent.Parent.Parent.KatActionsModeNums;

        ActionConfigGroups =
        [
            new KeyActionViewModel(this)
        ];
    }

    # region 添加删除

    [RelayCommand]
    private void AddActionConfig()
    {
        ActionConfigGroups.Add(new KeyActionViewModel(this));
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
    }

    public void InsertNextActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index+1, new KeyActionViewModel(this, ActionType.KeyBoard,
            VirtualKeyCode.None.ToString(),
            PressModeEnum.None, 1));
    }
    
    public void InsertNextDelayConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index+1, new KeyActionViewModel(this, ActionType.Delay,
            VirtualKeyCode.None.ToString(),
            PressModeEnum.None, 15));
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

    private readonly DialogOptions _dialogConfig = new DialogOptions()
    {
        Title = "选择组合键",
        Mode = DialogMode.Question,
        CanDragMove = true,
        IsCloseButtonVisible = true,
        ShowInTaskBar = false,
        CanResize = true
    };

    public void AddHotKeyActions(bool useCtrl,
        bool useWin,
        bool useAlt,
        bool useShift,
        HotKeyCodeEnum hotKey)
    {
        List<KeyActionViewModel> list = [];
        if (useCtrl) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "CONTROL", PressModeEnum.Press, 1));
        if (useWin) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "WIN", PressModeEnum.Press, 1));
        if (useAlt) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "MENU", PressModeEnum.Press, 1));
        if (useShift) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "SHIFT", PressModeEnum.Press, 1));
        list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, $"VK_{hotKey.ToStringFast()}", PressModeEnum.Click,
            1));
        if (useShift) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "SHIFT", PressModeEnum.Release, 1));
        if (useAlt) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "MENU", PressModeEnum.Release, 1));
        if (useWin) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "WIN", PressModeEnum.Release, 1));
        if (useCtrl) list.Add(new KeyActionViewModel(this, ActionType.KeyBoard, "CONTROL", PressModeEnum.Release, 1));
        ActionConfigGroups.Clear();
        list.Iter(ActionConfigGroups.Add);
    }

    #endregion
}
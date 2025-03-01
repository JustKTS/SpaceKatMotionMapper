using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using SpaceKatMotionMapper.Models;
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
            ActionConfigGroups.Add(new KeyActionViewModel(this, ActionType.KeyBoard, VirtualKeyCode.None.ToString(), PressModeEnum.None, 1 ));
        }
    }

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
                var actionConfigGroup = new KeyActionViewModel();
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
}
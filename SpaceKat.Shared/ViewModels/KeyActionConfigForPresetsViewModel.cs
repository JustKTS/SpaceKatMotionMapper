using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using WindowsInput;

namespace SpaceKat.Shared.ViewModels;

public partial class KeyActionConfigForPresetsViewModel : ObservableObject
{
    
    [ObservableProperty] private bool _isCustomDescription;
    [ObservableProperty] private string _description = string.Empty;

    public ObservableCollection<KeyActionConfigForPresetsViewModel>? Parent { get; init; }

    public ObservableCollection<KeyActionForPresetsViewModel> ActionConfigGroups { get; set; }

    public static IReadOnlyList<string> KeyNames => VirtualKeyHelpers.KeyNames;

    public bool IsAvailable => CheckIsAvailable();

    private bool CheckIsAvailable()
    {
        return ActionConfigGroups.All(e => e.IsAvailable);
    }

    public KeyActionConfigForPresetsViewModel()
    {
        ActionConfigGroups = [];

        ActionConfigGroups.CollectionChanged += (sender, e) =>
        {
            // 处理新增项：订阅PropertyChanged
            if (e.NewItems != null)
            {
                foreach (KeyActionForPresetsViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }

            // 处理移除项：取消订阅避免内存泄漏
            if (e.OldItems == null) return;

            foreach (KeyActionForPresetsViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        };

        AddActionConfig();
    }

    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(KeyActionForPresetsViewModel.IsAvailable))
        {
            OnPropertyChanged(nameof(IsAvailable));
        }
    }

    # region 添加删除

    [RelayCommand]
    private void AddActionConfig()
    {
        ActionConfigGroups.Add(new KeyActionForPresetsViewModel(this));
        OnPropertyChanged(nameof(IsAvailable));
    }

    [RelayCommand]
    private void RemoveActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.RemoveAt(index);
        if (ActionConfigGroups.Count == 0)
        {
            ActionConfigGroups.Add(new KeyActionForPresetsViewModel(this, ActionType.KeyBoard, VirtualKeyCode.None.ToString(),
                PressModeEnum.None, 1));
        }

        OnPropertyChanged(nameof(IsAvailable));
    }

    public void InsertNextActionConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index + 1, new KeyActionForPresetsViewModel(this, ActionType.KeyBoard,
            VirtualKeyCode.None.ToString(),
            PressModeEnum.None, 1));
        OnPropertyChanged(nameof(IsAvailable));
    }

    public void InsertNextDelayConfig(int index)
    {
        if (index < 0 || index >= ActionConfigGroups.Count) return;
        ActionConfigGroups.Insert(index + 1, new KeyActionForPresetsViewModel(this, ActionType.Delay,
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
                var actionConfigGroup = new KeyActionForPresetsViewModel(this);
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

    #region 移除自身
    [RelayCommand]
    private void RemoveSelf()
    {
        Parent?.Remove(this);
    }
    #endregion
    
}
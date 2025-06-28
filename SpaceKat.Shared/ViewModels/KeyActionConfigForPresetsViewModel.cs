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

    public ObservableCollection<KeyActionWithCommandViewModel> ActionConfigGroups { get; set; }

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
            foreach (var keyActionConfig in keyActionConfigs)
            {
                var actionConfigGroup = CreateKeyActionVm();
                var ret = actionConfigGroup.FromKeyActionConfig(keyActionConfig);
                if (!ret) return false;
                ActionConfigGroups.Add(actionConfigGroup);
            }

            OnPropertyChanged(nameof(IsAvailable));
            return true;
        }
        catch (Exception e)
        {
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
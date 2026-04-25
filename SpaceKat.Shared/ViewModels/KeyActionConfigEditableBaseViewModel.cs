using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.ViewModels;

public abstract partial class KeyActionConfigEditableBaseViewModel : ObservableObject
{
    protected ISharedKeyActionConfigStrategyProfile StrategyProfile { get; }

    public ObservableCollection<KeyActionWithCommandViewModel> ActionConfigGroups { get; }

    public bool IsAvailable => ActionConfigGroups.All(e => e.IsAvailable);

    protected KeyActionConfigEditableBaseViewModel(ISharedKeyActionConfigStrategyProfile? strategyProfile = null)
    {
        StrategyProfile = strategyProfile ?? new DefaultSharedKeyActionConfigStrategyProfile();
        ActionConfigGroups = [];
        ActionConfigGroups.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (KeyActionWithCommandViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }

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

    [RelayCommand]
    private void AddActionConfig()
    {
        ActionConfigGroups.Add(CreateManagedActionConfigVm());
        OnPropertyChanged(nameof(IsAvailable));
    }

    protected virtual KeyActionAvailabilityValidationOptions GetAvailabilityOptions() =>
        new(RequirePositiveScrollMultiplier: false);

    protected KeyActionWithCommandViewModel CreateManagedActionConfigVm(
        ActionType actionType = ActionType.KeyBoard,
        string key = KeyActionConstants.NoneKeyValue,
        PressModeEnum pressMode = PressModeEnum.None,
        int multiplier = 1)
    {
        var item = new KeyActionWithCommandViewModel(actionType, key, pressMode, multiplier,
            StrategyProfile.GetPressModePolicy(isSingleActionMode: false),
            StrategyProfile.AvailabilityValidator,
            GetAvailabilityOptions());
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
            AddActionConfig();
        }

        OnPropertyChanged(nameof(IsAvailable));
    }

    private void InsertNextActionConfig(KeyActionWithCommandViewModel vm)
    {
        var index = ActionConfigGroups.IndexOf(vm);
        ActionConfigGroups.Insert(index + 1, CreateManagedActionConfigVm());
        OnPropertyChanged(nameof(IsAvailable));
    }

    private void InsertNextDelayConfig(KeyActionWithCommandViewModel vm)
    {
        var index = ActionConfigGroups.IndexOf(vm);
        ActionConfigGroups.Insert(index + 1, CreateManagedActionConfigVm(ActionType.Delay,
            KeyActionConstants.NoneKeyValue,
            PressModeEnum.None,
            KeyActionConstants.MinDelayMultiplier));
        OnPropertyChanged(nameof(IsAvailable));
    }

    protected List<KeyActionConfig> ToKeyActionConfigListCore()
    {
        return ActionConfigGroups.Select(actionGroup => actionGroup.ToKeyActionConfig()).ToList();
    }

    protected bool FromKeyActionConfigCore(
        IReadOnlyList<KeyActionConfig> actionConfigs,
        Func<IReadOnlyList<KeyActionConfig>, bool>? afterLoad = null)
    {
        ActionConfigGroups.Clear();
        foreach (var keyActionConfig in actionConfigs)
        {
            var actionConfigGroup = CreateManagedActionConfigVm();
            var ret = actionConfigGroup.FromKeyActionConfig(keyActionConfig);
            if (!ret) return false;
            ActionConfigGroups.Add(actionConfigGroup);
        }

        if (afterLoad != null && !afterLoad(actionConfigs))
        {
            return false;
        }

        OnPropertyChanged(nameof(IsAvailable));
        return true;
    }
}



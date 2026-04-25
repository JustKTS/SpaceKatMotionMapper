using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.ViewModels;

public partial class KeyActionConfigForPresetsViewModel : KeyActionConfigEditableBaseViewModel
{
    [ObservableProperty] private bool _isCustomDescription;
    [ObservableProperty] private string _description = string.Empty;

    public ObservableCollection<KeyActionConfigForPresetsViewModel>? Parent { get; init; }

    public static IReadOnlyList<string> KeyNames => VirtualKeyHelpers.KeyNames;

    public KeyActionConfigForPresetsViewModel(ISharedKeyActionConfigStrategyProfile? strategyProfile = null) : base(strategyProfile)
    {
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
            return FromKeyActionConfigCore(actionConfigs);
        }
        catch (Exception)
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
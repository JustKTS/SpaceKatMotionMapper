using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using WindowsInput;

namespace ProgramSpecificConfigCreator.ViewModels;

public partial class CombinationKeysConfigViewModel : ViewModelBase
{
    public ObservableCollection<CombinationKeysConfigViewModel>? Parent { get; init; }
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _useCtrl;
    [ObservableProperty] private bool _useAlt;
    [ObservableProperty] private bool _useWin;
    [ObservableProperty] private bool _useShift;
    [ObservableProperty] private string _hotKey = string.Empty;

    [RelayCommand]
    private void RemoveSelf()
    {
        Parent?.Remove(this);
    }

    public CombinationKeysRecord ToRecord()
    {
        return new CombinationKeysRecord(UseCtrl, UseShift, UseAlt, UseWin, VirtualKeyHelpers.Parse(HotKey));
    }

    public void FromRecord(CombinationKeysRecord record)
    {
        UseCtrl = record.UseCtrl;
        UseShift = record.UseShift;
        UseAlt = record.UseAlt;
        UseWin = record.UseWin;
        HotKey = record.Key.GetWrappedName();
    }
}
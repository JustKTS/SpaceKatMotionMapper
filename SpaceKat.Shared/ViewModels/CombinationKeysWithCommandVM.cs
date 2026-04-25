using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
// ReSharper disable UnusedParameterInPartialMethod

namespace SpaceKat.Shared.ViewModels;

public partial class CombinationKeysWithCommandVM : ObservableObject
{
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _useCtrl;
    [ObservableProperty] private bool _useAlt;
    [ObservableProperty] private bool _useWin;
    [ObservableProperty] private bool _useShift;
    [ObservableProperty] private string _hotKey = string.Empty;

    partial void OnHotKeyChanged(string value)
    {
        SetKeys();
    }
    partial void OnUseCtrlChanged(bool value)
    {
        SetKeys();
    }
    partial void OnUseWinChanged(bool value)
    {
        SetKeys();
    }
    partial void OnUseShiftChanged(bool value)
    {
        SetKeys();
    }
    partial void OnUseAltChanged(bool value)
    {
        SetKeys();
    }
    
   

    public ICommand? RemoveSelfCommand { get; set; }

    public event EventHandler<CombinationKeysRecord>? OnKeysSetted;

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

    public void SetKeys()
    {
        OnKeysSetted?.Invoke(this, ToRecord());
    }
}
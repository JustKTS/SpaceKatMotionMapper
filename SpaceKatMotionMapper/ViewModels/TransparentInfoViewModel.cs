using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class TransparentInfoViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isAdjustMode;
    [ObservableProperty] private bool _isOtherInfo;
    [ObservableProperty] private string _otherInfo = string.Empty;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MotionName))]
    [NotifyPropertyChangedFor(nameof(PressModeName))]
    private KatAction _katAction = new KatAction(KatMotionEnum.Null, KatPressModeEnum.Null, 0);

    public string MotionName => KatAction.Motion.ToStringFast();
    public string PressModeName => KatAction.KatPressMode.ToStringFast();

    [RelayCommand]
    private static void SaveConfig()
    {
        App.GetRequiredService<TransparentInfoService>().StopAdjustInfoWindow();
    }
}
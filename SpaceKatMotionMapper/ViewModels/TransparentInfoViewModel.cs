using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Services;
using IBrush = Avalonia.Media.IBrush;

namespace SpaceKatMotionMapper.ViewModels;

public partial class TransparentInfoViewModel(TransparentInfoService transparentInfoService) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsAdjustMode { get; set; }

    [ObservableProperty]
    public partial bool IsOtherInfo { get; set; }

    [ObservableProperty]
    public partial string OtherInfo { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsActionInfo { get; set; }

    [ObservableProperty]
    public partial KeyActionConfig[] ActionInfo { get; set; } = [];

    [ObservableProperty]
    public partial double FontSize { get; set; } = 15;

    [ObservableProperty]
    public partial int DisappearTimeMs { get; set; } = 1500;

    [ObservableProperty]
    public partial int AnimationTimeMs { get; set; } = 250;

    partial void OnAnimationTimeMsChanged(int value)
    {
        transparentInfoService.AnimationTimeMs = value;
    }

    partial void OnDisappearTimeMsChanged(int value)
    {
        transparentInfoService.SetDisappearTime(value);
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundBrush))]
    public partial Color BackgroundColor { get; set; } = Colors.Gray;

    public IBrush BackgroundBrush => new SolidColorBrush(BackgroundColor);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontBrush))]
    public partial Color FontColor { get; set; } = Colors.White;

    public IBrush FontBrush => new SolidColorBrush(FontColor);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MotionName))]
    [NotifyPropertyChangedFor(nameof(PressModeName))]
    public partial KatMotion KatMotion { get; set; } = new(KatMotionEnum.Null, KatPressModeEnum.Null, 0);

    public string MotionName => KatMotion.Motion.ToStringFast(useMetadataAttributes:true);
    public string PressModeName => KatMotion.KatPressMode.ToStringFast(useMetadataAttributes:true);

 
    
    public void SaveConfig(int x, int y, double width, double height)
    {
        transparentInfoService.SaveConfigsAsync(
            x, y, width, height, BackgroundColor, FontColor, FontSize, DisappearTimeMs, AnimationTimeMs).ContinueWith(_ =>
        {
            transparentInfoService.StopAdjustInfoWindow();
        });
    }
}
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageExt.Pipes;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Services;
using IBrush = Avalonia.Media.IBrush;

namespace SpaceKatMotionMapper.ViewModels;

public partial class TransparentInfoViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isAdjustMode;
    [ObservableProperty] private bool _isOtherInfo;
    [ObservableProperty] private string _otherInfo = string.Empty;
    [ObservableProperty] private double _fontSize = 15;
    [ObservableProperty] private int _disappearTimeMs = 1500;
    [ObservableProperty] private int _animationTimeMs = 250;
    
    partial void OnAnimationTimeMsChanged(int value)
    {
        var service = App.GetRequiredService<TransparentInfoService>();
        service.AnimationTimeMs = value;
    }
    
    partial void OnDisappearTimeMsChanged(int value)
    {
        var service = App.GetRequiredService<TransparentInfoService>();
        service.SetDisappearTime(value);
    }

    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundBrush))]
    private Color _backgroundColor = Colors.Gray;

    public IBrush BackgroundBrush => new SolidColorBrush(BackgroundColor);
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontBrush))]
    private Color _fontColor = Colors.White;

    public IBrush FontBrush => new SolidColorBrush(FontColor);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MotionName))]
    [NotifyPropertyChangedFor(nameof(PressModeName))]
    private KatMotion _katMotion = new(KatMotionEnum.Null, KatPressModeEnum.Null, 0);

    public string MotionName => KatMotion.Motion.ToStringFast();
    public string PressModeName => KatMotion.KatPressMode.ToStringFast();

 
    
    public void SaveConfig(int x, int y, double width, double height)
    {
        var service = App.GetRequiredService<TransparentInfoService>();
        service.SaveConfigsAsync(
            x, y, width, height, BackgroundColor, FontColor, FontSize, DisappearTimeMs, AnimationTimeMs).ContinueWith(t =>
        {
            service.StopAdjustInfoWindow();
        });
    }
}
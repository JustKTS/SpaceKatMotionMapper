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
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(BackgroundBrush))]
    private Color _backgroundColor = Colors.Gray;

    public IBrush BackgroundBrush => new SolidColorBrush(BackgroundColor);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MotionName))]
    [NotifyPropertyChangedFor(nameof(PressModeName))]
    private KatAction _katAction = new KatAction(KatMotionEnum.Null, KatPressModeEnum.Null, 0);

    public string MotionName => KatAction.Motion.ToStringFast();
    public string PressModeName => KatAction.KatPressMode.ToStringFast();
    
    public void SaveConfig(int x, int y, double width, double height)
    {
        var service = App.GetRequiredService<TransparentInfoService>();
        service.SaveConfigsAsync(
            x, y, width, height, BackgroundColor).ContinueWith(t =>
        {
            service.StopAdjustInfoWindow();
        });
    }
}
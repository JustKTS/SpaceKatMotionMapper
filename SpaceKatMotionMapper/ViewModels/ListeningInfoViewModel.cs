using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ListeningInfoViewModel : ViewModelBase
{
    
    # region 当前状态

    [ObservableProperty] private string _katMotion = string.Empty;
    [ObservableProperty] private string _pressMode = string.Empty;
    [ObservableProperty] private int _repeatCount;


    private readonly KatMotionRecognizeService _katMotionRecognizeService;
    private readonly KatMotionActivateService _katMotionActivateService;

    private void StartKatListening()
    {
        _katMotionRecognizeService.DataReceived += (_, data) =>
        {
            if (!_katMotionActivateService.IsActivated) return;
            Dispatcher.UIThread.Invoke(() =>
            {
                KatMotion = data.Motion.ToStringFast(useMetadataAttributes:true);
                PressMode = data.KatPressMode.ToStringFast(useMetadataAttributes:true);
                RepeatCount = data.RepeatCount;
                
                try
                {
                    _transparentInfoService.DisplayKatMotion(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        };
    }

    #endregion

    # region 前台状态

    private readonly IPlatformForegroundProgramService _currentForeProgramHelper;
    [ObservableProperty] private ForeProgramInfo _currentForeProgramInfo = new();

    private void ForeProgramChangedCallback(object? sender, ForeProgramInfo data)
    {
        Dispatcher.UIThread.InvokeAsync(() => { CurrentForeProgramInfo = data; });
    }

    # endregion
    
    private readonly TransparentInfoService _transparentInfoService;

#if DEBUG
    public ListeningInfoViewModel() : this(null!, null!,  null!, null!)
    {
    }
#endif


    public ListeningInfoViewModel(
        IPlatformForegroundProgramService currentForeProgramHelper, 
        KatMotionRecognizeService katMotionRecognizeService,
        KatMotionActivateService katMotionActivateService,
        TransparentInfoService transparentInfoService)
    {
        _katMotionRecognizeService = katMotionRecognizeService;
        _katMotionActivateService = katMotionActivateService;
        _currentForeProgramHelper = currentForeProgramHelper;
        _transparentInfoService = transparentInfoService;
        _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangedCallback;
        StartKatListening();
    }
}
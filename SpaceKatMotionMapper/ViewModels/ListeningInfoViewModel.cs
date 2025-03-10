using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;
using Win32Helpers;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ListeningInfoViewModel : ViewModelBase
{
    
    # region 当前状态

    [ObservableProperty] private string _katMotion = string.Empty;
    [ObservableProperty] private string _pressMode = string.Empty;
    [ObservableProperty] private int _repeatCount;


    private readonly KatActionRecognizeService _katActionRecognizeService;
    private readonly KatActionActivateService _katActionActivateService;

    private void StartKatListening()
    {
        _katActionRecognizeService.DataReceived += (_, data) =>
        {
            if (!_katActionActivateService.IsActivated) return;
            Dispatcher.UIThread.Invoke(() =>
            {
                KatMotion = data.Motion.ToStringFast();
                PressMode = data.KatPressMode.ToStringFast();
                RepeatCount = data.RepeatCount;
                try
                {
                    _transparentInfoService.DisplayKatAction(data);
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

    private readonly CurrentForeProgramHelper _currentForeProgramHelper;
    [ObservableProperty] private ForeProgramInfo _currentForeProgramInfo = new();

    private void ForeProgramChangedCallback(object? sender, ForeProgramInfo data)
    {
        Dispatcher.UIThread.InvokeAsync(() => { CurrentForeProgramInfo = data; });
    }

    # endregion
    
    private readonly PopUpNotificationService _popUpNotificationService;
    private readonly TransparentInfoService _transparentInfoService;

#if DEBUG
    public ListeningInfoViewModel() : this(null!, null!, null!, null!, null!)
    {
    }
#endif


    public ListeningInfoViewModel(
        CurrentForeProgramHelper currentForeProgramHelper, PopUpNotificationService popUpNotificationService,
        KatActionRecognizeService katActionRecognizeService,
        KatActionActivateService katActionActivateService,
        TransparentInfoService transparentInfoService)
    {
        _katActionRecognizeService = katActionRecognizeService;
        _popUpNotificationService = popUpNotificationService;
        _katActionActivateService = katActionActivateService;
        _currentForeProgramHelper = currentForeProgramHelper;
        _transparentInfoService = transparentInfoService;
        _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangedCallback;
        StartKatListening();
    }
}
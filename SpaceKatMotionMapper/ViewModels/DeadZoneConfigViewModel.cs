using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class DeadZoneConfigViewModel : ViewModelBase
{
    private readonly KatActionRecognizeService _katActionRecognizeService;
    private readonly KatDeadZoneConfigService _katDeadZoneConfigService;

    [ObservableProperty] private bool _isDefault = true;

    [ObservableProperty] private Guid _id;

    public void UpdateById(Guid id)
    {
        IsDefault = false;
        Id = id;
    }
    
    public void UpdateByDefault()
    {
        IsDefault = true;
    }

    #region CTOR

#if DEBUG
    public DeadZoneConfigViewModel() : this(null!, null!)
    {
    }
#endif

    public DeadZoneConfigViewModel(KatActionRecognizeService katActionRecognizeService,
        KatDeadZoneConfigService katDeadZoneConfigService)
    {
        _katActionRecognizeService = katActionRecognizeService;
        _katDeadZoneConfigService = katDeadZoneConfigService;
        LoadDeadZoneAsync();
    }

    #endregion

    #region 死区设置

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneUpper))]
    private double _xDeadZoneUpper;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneUpper))]
    private double _yDeadZoneUpper;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneUpper))]
    private double _zDeadZoneUpper;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneUpper))]
    private double _pitchDeadZoneUpper;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneUpper))]
    private double _rollDeadZoneUpper;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneUpper))]
    private double _yawDeadZoneUpper;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneLower))]
    private double _xDeadZoneLower;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneLower))]
    private double _yDeadZoneLower;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneLower))]
    private double _zDeadZoneLower;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneLower))]
    private double _pitchDeadZoneLower;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneLower))]
    private double _rollDeadZoneLower;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DeadZoneLower))]
    private double _yawDeadZoneLower;

    public double[] DeadZoneUpper =>
    [
        XDeadZoneUpper, YDeadZoneUpper, ZDeadZoneUpper, RollDeadZoneUpper, PitchDeadZoneUpper, YawDeadZoneUpper
    ];

    public double[] DeadZoneLower =>
    [
        XDeadZoneLower, YDeadZoneLower, ZDeadZoneLower, RollDeadZoneLower, PitchDeadZoneLower, YawDeadZoneLower
    ];

    [RelayCommand]
    private void LoadDeadZoneAsync()
    {
        var deadZoneConfig = IsDefault
            ? _katDeadZoneConfigService.LoadDefaultDeadZoneConfigs() 
            : _katDeadZoneConfigService.LoadDeadZoneConfigs(Id);

        deadZoneConfig ??= _katDeadZoneConfigService.LoadDefaultDeadZoneConfigs();
        
        
        XDeadZoneUpper = deadZoneConfig.Upper[0];
        YDeadZoneUpper = deadZoneConfig.Upper[1];
        ZDeadZoneUpper = deadZoneConfig.Upper[2];
        RollDeadZoneUpper = deadZoneConfig.Upper[3];
        PitchDeadZoneUpper = deadZoneConfig.Upper[4];
        YawDeadZoneUpper = deadZoneConfig.Upper[5];
        XDeadZoneLower = deadZoneConfig.Lower[0];
        YDeadZoneLower = deadZoneConfig.Lower[1];
        ZDeadZoneLower = deadZoneConfig.Lower[2];
        RollDeadZoneLower = deadZoneConfig.Lower[3];
        PitchDeadZoneLower = deadZoneConfig.Lower[4];
        YawDeadZoneLower = deadZoneConfig.Lower[5];

        _katActionRecognizeService.SetDeadZone(DeadZoneUpper, DeadZoneLower);

    }

    [RelayCommand]
    private void ApplyDeadZone()
    {
        _katActionRecognizeService.SetDeadZone(DeadZoneUpper, DeadZoneLower);
    }

    [RelayCommand]
    private void SaveDeadZone()
    {
        var config = new KatDeadZoneConfig(DeadZoneUpper, DeadZoneLower);
        ApplyDeadZone();
        _ = IsDefault
            ? _katDeadZoneConfigService.SaveDefaultDeadZoneConfig(config)
            : _katDeadZoneConfigService.SaveDeadZoneConfig(config, Id);
    }

    #endregion

    #region 数据显示

    [ObservableProperty] private double _xData;
    [ObservableProperty] private double _yData;
    [ObservableProperty] private double _zData;
    [ObservableProperty] private double _pitchData;
    [ObservableProperty] private double _rollData;
    [ObservableProperty] private double _yawData;

    private readonly ManualResetEventSlim _stopEvent = new(false);

    public void StartAxesDataDisplay()
    {
        _stopEvent.Reset();
        Task.Run(() =>
        {
            while (!_stopEvent.IsSet)
            {
                var data = _katActionRecognizeService.KatDeviceData;
                Dispatcher.UIThread.Invoke(() =>
                {
                    XData = data.AxesData[0];
                    YData = data.AxesData[1];
                    ZData = data.AxesData[2];
                    RollData = data.AxesData[3];
                    PitchData = data.AxesData[4];
                    YawData = data.AxesData[5];
                });
            }
        });
    }

    public void StopAxesDataDisplay()
    {
        _stopEvent.Set();
    }

    #endregion
}
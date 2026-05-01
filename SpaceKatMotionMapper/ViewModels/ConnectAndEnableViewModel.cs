using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using Serilog;
using SpaceKat.Shared.Functions;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.States;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ConnectAndEnableViewModel : ObservableObject
{
    private readonly TransparentInfoService _transparentInfoService;
    private readonly PopUpNotificationService _popUpNotificationService;
    private readonly KatMotionRecognizeService _katMotionRecognizeService;
    private readonly IDeviceDataWrapper _deviceDataWrapper;
    private readonly GlobalStates _globalStates;

    public GlobalStates GlobalStates => _globalStates;

    public ConnectAndEnableViewModel(
        TransparentInfoService transparentInfoService,
        PopUpNotificationService popUpNotificationService,
        KatMotionRecognizeService katMotionRecognizeService,
        IDeviceDataWrapper deviceDataWrapper,
        GlobalStates globalStates)
    {
        _transparentInfoService = transparentInfoService;
        _popUpNotificationService = popUpNotificationService;
        _katMotionRecognizeService = katMotionRecognizeService;
        _deviceDataWrapper = deviceDataWrapper;
        _globalStates = globalStates;

        _globalStates.IsConnectionChanged += ConnectionChangeHandle;
        _globalStates.IsMapperEnableChanged += SwitchMapperEnable;
        _katMotionRecognizeService.ConnectionChanged += OnDeviceIsConnectedChange;
    }

    #region 连接情况

    private Task? _listenTask;

    [RelayCommand]
    private async Task ConnectBtn()
    {
        if (_globalStates.IsConnected)
        {
            await DisconnectDeviceAsync();
        }
        else
        {
            await ConnectDeviceAsync();
        }
    }

    private bool _isUseConnectButton;


    private void ConnectionChangeHandle(object? obj, bool isConnected)
    {
        if (_isUseConnectButton) return;
        if (!isConnected)
        {
            const string message = "连接断开";
            _transparentInfoService.DisplayOtherInfo(message);
            _popUpNotificationService.Pop(NotificationType.Warning, message);
            DisconnectDevice();
        }
        else
        {
            const string message = "连接成功";
            _transparentInfoService.DisplayOtherInfo(message);
            _popUpNotificationService.Pop(NotificationType.Success, message);

            _listenTask = new Task(async void () =>
            {
                try
                {
                    _katMotionRecognizeService.ExitEvent.Reset();
                    await _katMotionRecognizeService.StartRecognizeMotionAsync();
                }
                catch (Exception e)
                {
                    Log.Error(e, "[{ViewModel}] Error in motion recognition task", nameof(ConnectAndEnableViewModel));
                }
            }, TaskCreationOptions.LongRunning);
            _listenTask.Start();
        }
    }

    private async Task ConnectDeviceAsync()
    {
        var isConnected = await _deviceDataWrapper.Connect();

        if (isConnected) return;

        _isUseConnectButton = true;
        _popUpNotificationService.Pop(NotificationType.Warning, "未搜索到设备，是否已打开设备并配对成功？");
        _globalStates.IsConnected = true;
        _globalStates.IsConnected = false;
        _isUseConnectButton = false;
    }

    private void OnDeviceIsConnectedChange(object? sender, Result<bool, Exception> eitherValue)
    {
        if (eitherValue.IsSuccess)
        {
            Dispatcher.UIThread.InvokeAsync(() => _globalStates.IsConnected = eitherValue.Value);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => _globalStates.IsConnected = false);
        }
    }
       

    private void DisconnectDevice()
    {
        _katMotionRecognizeService.ExitEvent.Set();
        _listenTask?.Wait(TimeSpan.FromSeconds(3));
        _listenTask = null;
        _deviceDataWrapper.Disconnect();
    }

    private async Task DisconnectDeviceAsync()
    {
        _katMotionRecognizeService.ExitEvent.Set();
        if (_listenTask is not null)
        {
            await Task.WhenAny(_listenTask, Task.Delay(3000));
        }
        _listenTask = null;
        _deviceDataWrapper.Disconnect();
    }

    #endregion

    #region 映射启动情况

    private void SwitchMapperEnable(object? sender, bool e)
    {
        if (e)
        {
            OfficialWareConfigFunctions.CloseOfficialMapper().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Log.Error(t.Exception, "[{ViewModel}] Failed to close official mapper", nameof(ConnectAndEnableViewModel));
                else
                    _transparentInfoService.DisplayOtherInfo("官方映射已禁用");
            });
        }
        else
        {
            OfficialWareConfigFunctions.OpenOfficialMapper().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Log.Error(t.Exception, "[{ViewModel}] Failed to open official mapper", nameof(ConnectAndEnableViewModel));
                else
                    _transparentInfoService.DisplayOtherInfo("官方映射已启用");
            });
        }
    }

    #endregion
}
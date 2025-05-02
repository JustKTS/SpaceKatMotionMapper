using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageExt;
using Semi.Avalonia.Locale;
using SpaceKat.Shared.Functions;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Functions;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.States;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ConnectAndEnableViewModel : ObservableObject
{
    public ConnectAndEnableViewModel()
    {
        GlobalStates.IsConnectionChanged += ConnectionChangeHandle;
        GlobalStates.IsMapperEnableChanged += SwitchMapperEnable;
        _katMotionRecognizeService.ConnectionChanged += OnDeviceIsConnectedChange;
    }


    # region 服务相关

    private readonly TransparentInfoService _transparentInfoService = App.GetRequiredService<TransparentInfoService>();

    private readonly PopUpNotificationService _popUpNotificationService =
        App.GetRequiredService<PopUpNotificationService>();

    public static GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();

    # endregion

    #region 连接情况

    private readonly KatMotionRecognizeService _katMotionRecognizeService =
        App.GetRequiredService<KatMotionRecognizeService>();

    private readonly IDeviceDataWrapper _deviceDataWrapper =
        App.GetRequiredService<IDeviceDataWrapper>();

    private Task? _listenTask;

    [RelayCommand]
    private async Task ConnectBtn()
    {
        if (GlobalStates.IsConnected)
        {
            DisconnectDevice();
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
                    Debug.WriteLine(e);
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
        GlobalStates.IsConnected = true;
        GlobalStates.IsConnected = false;
        _isUseConnectButton = false;
    }

    private static void OnDeviceIsConnectedChange(object? sender, Either<Exception, bool> eitherValue)
    {
        eitherValue.Match(value => Dispatcher.UIThread.InvokeAsync(() => GlobalStates.IsConnected = value), ex =>
        {
            Dispatcher.UIThread.InvokeAsync(() => GlobalStates.IsConnected = false);
        });
    }
       

    private void DisconnectDevice()
    {
        _katMotionRecognizeService.ExitEvent.Set();
        _listenTask?.Wait();
        _listenTask = null;
        KatDeviceFunction.StopDevice();
    }

    #endregion

    #region 映射启动情况

    private void SwitchMapperEnable(object? sender, bool e)
    {
        if (e)
        {
            OfficialWareConfigFunctions.CloseOfficialMapper().ContinueWith(t =>
            {
                _transparentInfoService.DisplayOtherInfo("官方映射已禁用");
            });
        }
        else
        {
            OfficialWareConfigFunctions.OpenOfficialMapper().ContinueWith(t =>
            {
                _transparentInfoService.DisplayOtherInfo("官方映射已启用");
            });
        }
    }

    #endregion
}
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        
    }


    # region 服务相关

    private readonly TransparentInfoService _transparentInfoService = App.GetRequiredService<TransparentInfoService>();

    private readonly PopUpNotificationService _popUpNotificationService =
        App.GetRequiredService<PopUpNotificationService>();

    public static GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();

    # endregion

    #region 连接情况

    private readonly KatActionRecognizeService _katActionRecognizeService =
        App.GetRequiredService<KatActionRecognizeService>();

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

    private void ConnectionChangeHandle(object? obj, bool isConnected)
    {
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

            _listenTask = Task.Run(async () =>
            {
                _katActionRecognizeService.ExitEvent.Reset();

                try
                {
                    await _katActionRecognizeService.StartRecognizeMotionAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });
        }
    }

    private async Task ConnectDeviceAsync()
    {
        var isConnected = await _deviceDataWrapper.Connect();

        if (!isConnected)
        {
            _popUpNotificationService.Pop(NotificationType.Warning, "未搜索到设备，是否已连接好？");
            GlobalStates.IsConnected = true;
            GlobalStates.IsConnected = false;
            return;
        }

        GlobalStates.IsConnected = _deviceDataWrapper.IsConnected;
    }

    private void DisconnectDevice()
    {
        _katActionRecognizeService.ExitEvent.Set();
        _listenTask?.Wait();
        _listenTask = null;
        KatDeviceFunction.StopDevice();
        GlobalStates.IsConnected = false;
    }

    #endregion

    #region 映射启动情况

    private readonly OfficialMapperHotKeyService _officialMapperHotKeyService =
        App.GetRequiredService<OfficialMapperHotKeyService>();


    private void SwitchMapperEnable(object? sender, bool e)
    {
        if (e)
        {
            OfficialWareConfigFunctions.CloseOfficialMapper();
            _transparentInfoService.DisplayOtherInfo("官方映射已禁用");
        }
        else
        {
            OfficialWareConfigFunctions.OpenOfficialMapper();
            _transparentInfoService.DisplayOtherInfo("官方映射已启用");
        }
    }
    
    
    #endregion
}
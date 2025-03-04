using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Functions;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;
using Win32Helpers;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ListeningInfoViewModel : ViewModelBase
{
    #region 连接情况

    [ObservableProperty] private bool _isConnected;

    private Task? _listenTask;

    [RelayCommand]
    private async Task ConnectBtn()
    {
        if (IsConnected)
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
        IsConnected = isConnected;
        if (!isConnected)
        {
            const string message = "连接断开";
            _transparentInfoService.DisplayOtherInfo(message);
            _popUpNotificationService.Pop(NotificationType.Warning, message);
            DisconnectDevice();
        }
        else
        {
            IsConnected = _deviceDataWrapper.IsConnected;
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
            IsConnected = true;
            IsConnected = false;
            return;
        }

        IsConnected = _deviceDataWrapper.IsConnected;
    }

    private void DisconnectDevice()
    {
        _katActionRecognizeService.ExitEvent.Set();
        _listenTask?.Wait();
        _listenTask = null;
        KatDeviceFunction.StopDevice();
        IsConnected = false;
    }

    #endregion

    #region 禁用官方映射

    [ObservableProperty] private bool _isOfficialMapperOff;
    private readonly OfficialMapperSwitchService _officialMapperSwitchService;
    private readonly KatActionActivateService _katActionActivateService;
    private readonly ILocalSettingsService _localSettingsService;


    [ObservableProperty] private bool _useCtrl = true;
    [ObservableProperty] private bool _useAlt = true;
    [ObservableProperty] private bool _useShift;
    [ObservableProperty] private HotKeyCodeEnum _hotKey = HotKeyCodeEnum.D;

    [ObservableProperty] private KatButtonEnum _selectedKatButton = KatButtonEnum.None;

    public static KatButtonEnum[] KatButtonList => KatButtonEnumExtensions.GetValues();

    public static HotKeyCodeEnum[] HotKeyCodes => HotKeyCodeEnumExtensions.GetValues();


    partial void OnIsOfficialMapperOffChanged(bool value)
    {
        if (value)
        {
            OfficialWareConfigFunctions.CloseOfficialMapper();
            _katActionActivateService.IsActivated = true;
            _transparentInfoService.DisplayOtherInfo("官方映射已禁用");
        }
        else
        {
            OfficialWareConfigFunctions.OpenOfficialMapper();
            _katActionActivateService.IsActivated = false;
            _transparentInfoService.DisplayOtherInfo("官方映射已启用");
        }
    }

    private void SaveHotKey()
    {
        _ = _localSettingsService.SaveSettingAsync(nameof(HotKeyRecord),
            new HotKeyRecord(UseCtrl, UseAlt, UseShift, HotKey, SelectedKatButton));
    }

    private void LoadHotKey()
    {
        _localSettingsService.ReadSettingAsync<HotKeyRecord>(nameof(HotKeyRecord)).ContinueWith(task =>
        {
            if (task.Result is not { } hotKey) return;
            UseCtrl = hotKey.UseCtrl;
            UseAlt = hotKey.UseAlt;
            UseShift = hotKey.UseShift;
            HotKey = hotKey.HotKey;
            SelectedKatButton = hotKey.BindKatButtonEnum;
        });
    }

    [RelayCommand]
    private void RegisterHotKey()
    {
        if (UseShift || UseAlt || UseCtrl)
        {
            var ret = _officialMapperSwitchService.RegisterHotKeyWrapper(UseCtrl, UseAlt, UseShift, HotKey,
                SelectedKatButton);
            var ret2 = ret.Match(s =>
            {
                if (!s)
                {
                    _popUpNotificationService.Pop(NotificationType.Warning, "注册热键失败");
                }

                return s;
            }, ex =>
            {
                _popUpNotificationService.Pop(NotificationType.Warning, "注册热键失败");
                return false;
            });
            if (ret2) SaveHotKey();
        }
        else
        {
            _popUpNotificationService.Pop(NotificationType.Warning, "至少选择一个特殊键");
        }
    }

    #endregion

    # region 当前状态

    [ObservableProperty] private string _katMotion = string.Empty;
    [ObservableProperty] private string _pressMode = string.Empty;
    [ObservableProperty] private int _repeatCount;


    private readonly KatActionRecognizeService _katActionRecognizeService;
    private readonly IDeviceDataWrapper _deviceDataWrapper;

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

    # region 自动禁用

    private readonly AutoDisableService _autoDisableService;
    private readonly OverlayDialogOptions _options = new()
    {
        FullScreen = false,
        HorizontalAnchor = HorizontalPosition.Center,
        VerticalAnchor = VerticalPosition.Center,
        HorizontalOffset = 0.0,
        VerticalOffset = 0.0,
        Mode = DialogMode.None,
        Buttons = DialogButton.None,
        Title = "",
        CanLightDismiss = true,
        CanDragMove = true,
        IsCloseButtonVisible = true,
        CanResize = true
    };
    
    [RelayCommand]
    private async Task ShowAutoDisplay()
    {
        await OverlayDialog.ShowModal<AutoDisableConfigView, AutoDisableViewModel>(
            App.GetRequiredService<AutoDisableViewModel>(), MainWindow.LocalHost, options: _options);
    }

    #endregion

    private readonly PopUpNotificationService _popUpNotificationService;
    private readonly TransparentInfoService _transparentInfoService;

#if DEBUG
    public ListeningInfoViewModel() : this(null!,null!, null!, null!, null!, null!, null!, null!, null!)
    {
    }
#endif


    public ListeningInfoViewModel(
        IDeviceDataWrapper deviceDataWrapper,
        CurrentForeProgramHelper currentForeProgramHelper, PopUpNotificationService popUpNotificationService,
        KatActionRecognizeService katActionRecognizeService,
        OfficialMapperSwitchService officialMapperSwitchService,
        KatActionActivateService katActionActivateService,
        ILocalSettingsService localSettingsService,
        TransparentInfoService transparentInfoService,
        AutoDisableService autoDisableService)
    {
        _deviceDataWrapper = deviceDataWrapper;
        _katActionRecognizeService = katActionRecognizeService;
        _popUpNotificationService = popUpNotificationService;
        _officialMapperSwitchService = officialMapperSwitchService;
        _katActionActivateService = katActionActivateService;
        _localSettingsService = localSettingsService;
        _currentForeProgramHelper = currentForeProgramHelper;
        _transparentInfoService = transparentInfoService;
        _autoDisableService = autoDisableService;
        _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangedCallback;
        StartKatListening();
        LoadHotKey();
        _katActionRecognizeService.ConnectionChanged += ConnectionChangeHandle;
    }
}

public record HotKeyRecord(
    bool UseCtrl,
    bool UseAlt,
    bool UseShift,
    HotKeyCodeEnum HotKey,
    KatButtonEnum BindKatButtonEnum);
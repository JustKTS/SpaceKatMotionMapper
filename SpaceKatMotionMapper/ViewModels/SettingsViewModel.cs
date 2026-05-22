using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetaKeyPresetsEditor.Views;
using PlatformAbstractions;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.States;
using SpaceKatHIDWrapper.DeviceHIDSpecs;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IGlobalStates _globalStates;
    private readonly ITransparentInfoService _transparentInfoService;
    private readonly IPopUpNotificationService _popUpNotificationService;
    private readonly IFileExplorerService _fileExplorerService;
    private readonly IDeviceDataWrapper _deviceDataWrapper;
    private readonly TransparentInfoViewModel _transparentInfoViewModel;
    private readonly IOfficialMapperHotKeyService _officialMapperHotKeyService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IPlatformAutostartService _platformAutostartService;
    private readonly AutoDisableViewModel _autoDisableViewModel;
    private readonly MainWindow _mainWindow;
    private readonly PresetsEditorMainWindow _presetsEditorMainWindow;
    private readonly FavPresetsEditorViewModel _favPresetsEditorViewModel;

    public SettingsViewModel(
        IGlobalStates globalStates,
        ITransparentInfoService transparentInfoService,
        IPopUpNotificationService popUpNotificationService,
        IFileExplorerService fileExplorerService,
        IDeviceDataWrapper deviceDataWrapper,
        TransparentInfoViewModel transparentInfoViewModel,
        IOfficialMapperHotKeyService officialMapperHotKeyService,
        ILocalSettingsService localSettingsService,
        IPlatformAutostartService platformAutostartService,
        AutoDisableViewModel autoDisableViewModel,
        MainWindow mainWindow,
        PresetsEditorMainWindow presetsEditorMainWindow,
        FavPresetsEditorViewModel favPresetsEditorViewModel)
    {
        _globalStates = globalStates;
        _transparentInfoService = transparentInfoService;
        _popUpNotificationService = popUpNotificationService;
        _fileExplorerService = fileExplorerService;
        _deviceDataWrapper = deviceDataWrapper;
        _transparentInfoViewModel = transparentInfoViewModel;
        _officialMapperHotKeyService = officialMapperHotKeyService;
        _localSettingsService = localSettingsService;
        _platformAutostartService = platformAutostartService;
        _autoDisableViewModel = autoDisableViewModel;
        _mainWindow = mainWindow;
        _presetsEditorMainWindow = presetsEditorMainWindow;
        _favPresetsEditorViewModel = favPresetsEditorViewModel;
        AutoDisableViewModel = autoDisableViewModel;

        DisappearTimeMs = _transparentInfoViewModel.DisappearTimeMs;
        AnimationTimeMs = _transparentInfoViewModel.AnimationTimeMs;
    }

    public IGlobalStates GlobalStates => _globalStates;
    
    # region 透明通知窗设置

    [ObservableProperty] private int _disappearTimeMs;
    [ObservableProperty] private int _animationTimeMs;

    [RelayCommand]
    private async Task SetTransparentInfoWindowTimes()
    {
        _transparentInfoViewModel.DisappearTimeMs = DisappearTimeMs;
        _transparentInfoViewModel.AnimationTimeMs = AnimationTimeMs;
        await _transparentInfoService.UpdateTimeConfigs(DisappearTimeMs, AnimationTimeMs);
    }
    
    [RelayCommand]
    private void AdjustTransparentInfoWindow()
    {
        if (_transparentInfoViewModel.IsAdjustMode)
        {
            return;
        }

        _transparentInfoService.StartAdjustInfoWindow();
    }

    [RelayCommand]
    private void HideTransparentInfoWindow()
    {
        if (!_transparentInfoViewModel.IsAdjustMode)
        {
            return;
        }

        _transparentInfoService.StopAdjustInfoWindow();
    }

    # endregion

    #region 禁用官方映射


    [ObservableProperty] private bool _useCtrl = true;
    [ObservableProperty] private bool _useAlt = true;
    [ObservableProperty] private bool _useShift;
    [ObservableProperty] private KeyCodeWrapper _hotKey = KeyCodeWrapper.D;

    [ObservableProperty] private KatButtonEnum _selectedKatButton = KatButtonEnum.None;

    public static KatButtonEnum[] KatButtonList => KatButtonEnumExtensions.GetValues();

    public static IReadOnlyList<string> HotKeyCodes => VirtualKeyHelpers.KeyNames;

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

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                RegisterHotKeyCommand.Execute(null);
            });
        });
        
    }

    [RelayCommand]
    private async Task RegisterHotKey()
    {
        if (UseShift || UseAlt || UseCtrl)
        {
            var ret = await _officialMapperHotKeyService.RegisterHotKeyWrapper(UseCtrl, UseAlt, UseShift, HotKey,
                SelectedKatButton);
            if (ret.IsSuccess)
            {
                if (!ret.Value)
                {
                    _popUpNotificationService.Pop(NotificationType.Warning, "注册热键失败");
                }
                else
                {
                    _popUpNotificationService.Pop(NotificationType.Success, "注册热键成功");
                    SaveHotKey();
                }
            }
            else
            {
                _popUpNotificationService.Pop(NotificationType.Warning, $"注册热键失败：{ret.Error.Message}");
            }
        }
        else
        {
            _popUpNotificationService.Pop(NotificationType.Warning, "至少选择一个特殊键");
        }
    }

    #endregion

    # region 自动禁用官方映射

    public AutoDisableViewModel AutoDisableViewModel { get; }

    # endregion

    # region 配置文件夹

    [RelayCommand]
    private void OpenConfigFolder()
    {
        _fileExplorerService.OpenPath(GlobalPaths.AppDataPath);
    }

    [RelayCommand]
    private void OpenLogFolder()
    {
        _fileExplorerService.OpenPath(GlobalPaths.AppLogPath);
    }

    # endregion

    # region 3DConnexion 设备支持

    [ObservableProperty] private bool _isThreeDConnexionEnabled;

    partial void OnIsThreeDConnexionEnabledChanged(bool value)
    {
        _ = _localSettingsService.SaveSettingAsync(nameof(IsThreeDConnexionEnabled), value);

        try
        {
            DeviceHidSpecDict.Reload(GlobalPaths.AppDataPath, value);
        }
        catch (Exception ex)
        {
            var dialog = new ConfigReplaceDialog(ex.Message);
            dialog.ShowDialog(_mainWindow);

            if (dialog.ShouldReplace)
            {
                try
                {
                    DeviceHidSpecDict.ResetToDefault(GlobalPaths.AppDataPath, value);
                    DeviceHidSpecDict.Reload(GlobalPaths.AppDataPath, value);
                    _popUpNotificationService.Pop(NotificationType.Success,
                        "已使用内置默认配置替换");
                }
                catch (Exception ex2)
                {
                    _popUpNotificationService.Pop(NotificationType.Error,
                        $"替换失败: {ex2.Message}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IsThreeDConnexionEnabled = !value;
                    });
                }
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsThreeDConnexionEnabled = !value;
                });
            }
            return;
        }

        if (_deviceDataWrapper.IsConnected)
        {
            _deviceDataWrapper.Disconnect();
            _popUpNotificationService.Pop(NotificationType.Information,
                value
                    ? "3DConnexion 设备支持已开启，已断开设备，请重新连接"
                    : "3DConnexion 设备支持已关闭，已断开设备，请重新连接");
        }
    }

    private void LoadThreeDConnexionSetting()
    {
        Task.Run(async () =>
        {
            try
            {
                var enabled = await _localSettingsService.ReadSettingAsync<bool>(nameof(IsThreeDConnexionEnabled));
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsThreeDConnexionEnabled = enabled;
                });
            }
            catch (Exception)
            {
            }
        });
    }

    # endregion
    
    #region 开机自启动

    [ObservableProperty] private bool _isAutostartEnabled;
    [ObservableProperty] private bool _isAutostartAvailable;

    partial void OnIsAutostartEnabledChanged(bool value)
    {
        _ = _localSettingsService.SaveSettingAsync(nameof(IsAutostartEnabled), value);
        _platformAutostartService.IsAutostartEnabled = value;
    }

    private void LoadAutostartSetting()
    {
        IsAutostartAvailable = _platformAutostartService.IsAvailable;

        Task.Run(async () =>
        {
            try
            {
                var enabled = await _localSettingsService.ReadSettingAsync<bool>(nameof(IsAutostartEnabled));
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsAutostartEnabled = enabled;
                });
            }
            catch (Exception)
            {
            }
        });
    }

    #endregion

    # region 启动时加载

    public void LoadInStart()
    {
        LoadHotKey();
        AutoDisableViewModel.LoadInfos();
        LoadThreeDConnexionSetting();
        LoadAutostartSetting();
    }

    #endregion

    #region 各应用预设快捷键配置工具
    
    [RelayCommand]
    private void OpenProgramSpecificConfigCreator()
    {
        _presetsEditorMainWindow.Show();
    }   
    
    [RelayCommand]
    private void OpenMetaKeysConfigFolder()
    {
        _fileExplorerService.OpenPath(GlobalPaths.MetaKeysConfigPath);
    }

    private static readonly DialogOptions FavEditorDialogOptions = new()
    {
        StartupLocation = WindowStartupLocation.CenterOwner,
        Mode = DialogMode.Info,
        Button = DialogButton.None,
        IsCloseButtonVisible = true,
        ShowInTaskBar = false,
        CanDragMove = true,
        CanResize = true
    };
    
    [RelayCommand]
    private async Task OpenFavPresetsEditor()
    {
        await Dialog.ShowCustomAsync<FavPresetsEditorView, FavPresetsEditorViewModel, object>(
            _favPresetsEditorViewModel, _mainWindow, FavEditorDialogOptions
            );
    }
    
    [RelayCommand]
    private async Task GetPresetsFromInternet()
    {
        var ret = await DownloadMetaKeyPresetsHelper.DownloadAndCopyMetaKeyPresetsAsync();
        if (ret.IsSuccess)
        {
            _popUpNotificationService.Pop(NotificationType.Success, "预设下载成功");
        }
        else
        {
            _popUpNotificationService.Pop(NotificationType.Error, $"预设下载失败：{ret.Error.Message}");
        }
    }
    
    #endregion
}
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
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();
    private readonly TransparentInfoService _transparentInfoService = App.GetRequiredService<TransparentInfoService>();

    private readonly PopUpNotificationService _popUpNotificationService =
        App.GetRequiredService<PopUpNotificationService>();

    private readonly IFileExplorerService _fileExplorerService = App.GetRequiredService<IFileExplorerService>();

    private readonly IDeviceDataWrapper _deviceDataWrapper = App.GetRequiredService<IDeviceDataWrapper>();

    public SettingsViewModel()
    {
        DisappearTimeMs = _transparentInfoViewModel.DisappearTimeMs;
        AnimationTimeMs = _transparentInfoViewModel.AnimationTimeMs;
    }
    
    # region 透明通知窗设置
    private readonly TransparentInfoViewModel _transparentInfoViewModel =
        App.GetRequiredService<TransparentInfoViewModel>();

    [ObservableProperty] private int _disappearTimeMs;
    [ObservableProperty] private int _animationTimeMs;

    [RelayCommand]
    private async Task SetTransparentInfoWindowTimes()
    {
        // TODO: 此处时间设置冗余，window打开时会重新读取LocalConfig
        var infoService = App.GetRequiredService<TransparentInfoService>();
        _transparentInfoViewModel.DisappearTimeMs = DisappearTimeMs;
        _transparentInfoViewModel.AnimationTimeMs = AnimationTimeMs;
        await infoService.UpdateTimeConfigs(DisappearTimeMs, AnimationTimeMs);
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

    private readonly IOfficialMapperHotKeyService _officialMapperHotKeyService =
        App.GetRequiredService<IOfficialMapperHotKeyService>();
    
    private readonly ILocalSettingsService _localSettingsService = App.GetRequiredService<ILocalSettingsService>();


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
            _ = ret.Match(s =>
            {
                if (!s)
                {
                    _popUpNotificationService.Pop(NotificationType.Warning, "注册热键失败");
                }

                _popUpNotificationService.Pop(NotificationType.Success, "注册热键成功");
                SaveHotKey();
                return true;
            }, _ =>
            {
                _popUpNotificationService.Pop(NotificationType.Warning, "注册热键失败");
                return false;
            });
        }
        else
        {
            _popUpNotificationService.Pop(NotificationType.Warning, "至少选择一个特殊键");
        }
    }

    #endregion

    # region 自动禁用官方映射

    public static AutoDisableViewModel AutoDisableViewModel => App.GetRequiredService<AutoDisableViewModel>();

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
            dialog.ShowDialog(App.GetRequiredService<MainWindow>());

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
        var enabled = Task.Run<bool?>(async () =>
                await _localSettingsService.ReadSettingAsync<bool>(nameof(IsThreeDConnexionEnabled)))
            .GetAwaiter().GetResult();
        IsThreeDConnexionEnabled = enabled ?? false;
    }

    # endregion
    
    # region 启动时加载

    public void LoadInStart()
    {
        LoadHotKey();
        AutoDisableViewModel.LoadInfos();
        LoadThreeDConnexionSetting();
    }

    #endregion

    #region 各应用预设快捷键配置工具
    
    [RelayCommand]
    private static void OpenProgramSpecificConfigCreator()
    {
        var mainWindow = App.GetRequiredService<PresetsEditorMainWindow>();
        mainWindow.Show();
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
    private static async Task OpenFavPresetsEditor()
    {
        await Dialog.ShowCustomAsync<FavPresetsEditorView, FavPresetsEditorViewModel, object>(
            App.GetRequiredService<FavPresetsEditorViewModel>(), App.GetRequiredService<MainWindow>(), FavEditorDialogOptions
            );
    }
    
    [RelayCommand]
    private async Task GetPresetsFromInternet()
    {
        var ret = await DownloadMetaKeyPresetsHelper.DownloadAndCopyMetaKeyPresetsAsync();
        _ = ret.Match(_ =>
        {
            _popUpNotificationService.Pop(NotificationType.Success, "预设下载成功");
            return true;
        }, ex =>
        {
            _popUpNotificationService.Pop(NotificationType.Error, $"预设下载失败：{ex.Message}");
            return false;
        });
    }
    
    #endregion
}
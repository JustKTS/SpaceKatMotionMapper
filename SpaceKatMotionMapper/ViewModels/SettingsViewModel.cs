using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;

namespace SpaceKatMotionMapper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();
    private readonly TransparentInfoService _transparentInfoService = App.GetRequiredService<TransparentInfoService>();

    private readonly PopUpNotificationService _popUpNotificationService =
        App.GetRequiredService<PopUpNotificationService>();
    
    # region 透明通知窗设置

    private readonly TransparentInfoViewModel _transparentInfoViewModel =
        App.GetRequiredService<TransparentInfoViewModel>();

    [RelayCommand]
    private void AdjustTransparentInfoWindow()
    {
        if (_transparentInfoViewModel.IsAdjustMode)
        {
            return;
        }

        _transparentInfoService.StartAdjustInfoWindow();
    }

    # endregion

    #region 禁用官方映射


    private readonly OfficialMapperHotKeyService _officialMapperHotKeyService =
        App.GetRequiredService<OfficialMapperHotKeyService>();

    private readonly KatActionActivateService _katActionActivateService =
        App.GetRequiredService<KatActionActivateService>();

    private readonly ILocalSettingsService _localSettingsService = App.GetRequiredService<ILocalSettingsService>();


    [ObservableProperty] private bool _useCtrl = true;
    [ObservableProperty] private bool _useAlt = true;
    [ObservableProperty] private bool _useShift;
    [ObservableProperty] private HotKeyCodeEnum _hotKey = HotKeyCodeEnum.D;

    [ObservableProperty] private KatButtonEnum _selectedKatButton = KatButtonEnum.None;

    public static KatButtonEnum[] KatButtonList => KatButtonEnumExtensions.GetValues();

    public static HotKeyCodeEnum[] HotKeyCodes => HotKeyCodeEnumExtensions.GetValues();

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
            var ret = _officialMapperHotKeyService.RegisterHotKeyWrapper(UseCtrl, UseAlt, UseShift, HotKey,
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

    # region 自动禁用官方映射

    public AutoDisableViewModel AutoDisableViewModel => App.GetRequiredService<AutoDisableViewModel>();

    # endregion

    # region 配置文件夹

    [RelayCommand]
    private static void OpenConfigFolder()
    {
        _ = Process.Start("explorer.exe", GlobalPaths.AppDataPath);
    }

    [RelayCommand]
    private static void OpenLogFolder()
    {
        _ = Process.Start("explorer.exe", GlobalPaths.AppLogPath);
    }

    # endregion
    
    # region 启动时加载
    
    public void LoadInStart()
    {
        LoadHotKey();
    }
    #endregion
}

public record HotKeyRecord(
    bool UseCtrl,
    bool UseAlt,
    bool UseShift,
    HotKeyCodeEnum HotKey,
    KatButtonEnum BindKatButtonEnum);
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgramSpecificConfigCreator.Views;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.States;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using WindowsInput;

namespace SpaceKatMotionMapper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();
    private readonly TransparentInfoService _transparentInfoService = App.GetRequiredService<TransparentInfoService>();

    private readonly PopUpNotificationService _popUpNotificationService =
        App.GetRequiredService<PopUpNotificationService>();

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

    private readonly OfficialMapperHotKeyService _officialMapperHotKeyService =
        App.GetRequiredService<OfficialMapperHotKeyService>();

    private readonly KatMotionActivateService _katMotionActivateService =
        App.GetRequiredService<KatMotionActivateService>();

    private readonly ILocalSettingsService _localSettingsService = App.GetRequiredService<ILocalSettingsService>();


    [ObservableProperty] private bool _useCtrl = true;
    [ObservableProperty] private bool _useAlt = true;
    [ObservableProperty] private bool _useShift;
    [ObservableProperty] private VirtualKeyCode _hotKey = VirtualKeyCode.VK_D;

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
            }, ex =>
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
        AutoDisableViewModel.LoadInfos();
    }

    #endregion

    #region 各应用预设快捷键配置工具
    
    [RelayCommand]
    private static void OpenProgramSpecificConfigCreator()
    {
        var mainWindow = App.GetRequiredService<ProgramSpecificConfigMainWindow>();
        mainWindow.Show();
    }   
    

    #endregion
}
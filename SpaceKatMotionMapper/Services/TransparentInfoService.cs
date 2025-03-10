using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using Timer = System.Timers.Timer;

namespace SpaceKatMotionMapper.Services;

public class TransparentInfoService
{
    private const string EnableStr = "IsTransparentInfoEnable";
    private const string ConfigStr = "TransparentInfoWindowConfig";
    public bool IsTransparentInfoEnable { get; set; } = true;
    private bool _isWindowShow;

    private readonly Timer _timer = new()
    {
        AutoReset = false,
        Interval = 1500
    };

    private readonly TransparentInfoWindow _transparentInfoWindow;
    private readonly ILocalSettingsService _localSettingsService;

    public TransparentInfoService(
        TransparentInfoWindow transparentInfoWindow,
        ILocalSettingsService localSettingsService)
    {
        _transparentInfoWindow = transparentInfoWindow;
        _localSettingsService = localSettingsService;

        App.GetRequiredService<GlobalStates>().IsTransparentInfoEnableChanged += (_, value) =>
        {
            IsTransparentInfoEnable = value;
            _localSettingsService.SaveSettingAsync(EnableStr, value);
        };

        _timer.Elapsed += (_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                transparentInfoWindow.SetOpacity(0);
                await Task.Delay(250);
                transparentInfoWindow.Hide();
            });
            _isWindowShow = false;
        };
    }

    public void SetHideTimeout(int timeoutMs)
    {
        _timer.Interval = timeoutMs;
    }


    public void DisplayKatAction(KatAction action)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var vm = App.GetRequiredService<TransparentInfoViewModel>();
            vm.KatAction = action;
            vm.IsOtherInfo = false;
        });
        UpdateWindowState();
    }

    public void DisplayOtherInfo(string info)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var vm = App.GetRequiredService<TransparentInfoViewModel>();
            vm.OtherInfo = info;
            vm.IsOtherInfo = true;
        });
        UpdateWindowState();
    }

    private void UpdateWindowState()
    {
        if (!IsTransparentInfoEnable) return;

        var vm = App.GetRequiredService<TransparentInfoViewModel>();
        if (vm.IsAdjustMode) return;

        if (_isWindowShow)
        {
            _timer.Enabled = false;
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                _transparentInfoWindow.Show();
                _transparentInfoWindow.SetOpacity(1);
            });
            _isWindowShow = true;
        }

        _timer.Enabled = true;
    }

    public void StartAdjustInfoWindow()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var vm = App.GetRequiredService<TransparentInfoViewModel>();
            vm.IsAdjustMode = true;
            _transparentInfoWindow.ShowActivated = true;
            _transparentInfoWindow.IsManagedResizerVisible = true;
            _transparentInfoWindow.IsHitTestVisible = true;
            _transparentInfoWindow.Show();
            _transparentInfoWindow.SetOpacity(1);
        });
    }

    public void StopAdjustInfoWindow()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var vm = App.GetRequiredService<TransparentInfoViewModel>();
            _transparentInfoWindow.IsHitTestVisible = false;
            _transparentInfoWindow.IsManagedResizerVisible = false;
            _transparentInfoWindow.ShowActivated = false;
            vm.IsAdjustMode = false;
            _transparentInfoWindow.SetOpacity(0);
            _transparentInfoWindow.Hide();
        });
    }

    public async Task SaveConfigsAsync(int x, int y, double width, double height, Color color)
    {
        await _localSettingsService.SaveSettingAsync(ConfigStr,
            new TransparentInfoWindowConfig(
                x,
                y,
                width,
                height,
                color.ToUInt32()
            ));
    }

    public async Task<TransparentInfoWindowConfig?> LoadConfigs()
    {
        var isEnable = await _localSettingsService.ReadSettingAsync<bool?>(EnableStr);
        Dispatcher.UIThread.Invoke(() =>
        {
            App.GetRequiredService<GlobalStates>().IsTransparentInfoEnable = isEnable ?? true;
        });
        return await _localSettingsService.ReadSettingAsync<TransparentInfoWindowConfig>(ConfigStr);
    }
}
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
    private bool IsTransparentInfoEnable { get; set; } = true;

    private TransparentInfoWindow? _window = null;
    private bool _isWindowShow;
    
    public int AnimationTimeMs { get; set; } = 250;


    private readonly Timer _timer = new()
    {
        AutoReset = false,
        Interval = 1500
    };

    private readonly ILocalSettingsService _localSettingsService;

    public TransparentInfoService(
        ILocalSettingsService localSettingsService)
    {
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
                _window ??= App.GetRequiredService<TransparentInfoWindow>();
                _window.SetOpacity(0);
                await Task.Delay(AnimationTimeMs);
                _window.Close();
                _window = null;
            });
            _isWindowShow = false;
        };
    }

    public void SetDisappearTime(int timeMs)
    {
        _timer.Interval = timeMs;
    }

    public void DisplayKatMotion(KatMotionWithTimeStamp motion)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var vm = App.GetRequiredService<TransparentInfoViewModel>();
            vm.KatMotion = motion.ToKatMotion();
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
                _window ??= App.GetRequiredService<TransparentInfoWindow>();

                _window.Show();
                _window.SetOpacity(1);
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
            _window ??= App.GetRequiredService<TransparentInfoWindow>();
            _window.ShowActivated = true;
            _window.IsManagedResizerVisible = true;
            _window.IsHitTestVisible = true;
            _window.Show();
            _window.SetOpacity(1);
        });
    }

    public void StopAdjustInfoWindow()
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            _window ??= App.GetRequiredService<TransparentInfoWindow>();
            var vm = App.GetRequiredService<TransparentInfoViewModel>();
            _window.IsHitTestVisible = false;
            _window.IsManagedResizerVisible = false;
            _window.ShowActivated = false;
            vm.IsAdjustMode = false;
            _window.SetOpacity(0);
            await Task.Delay(AnimationTimeMs);
            _window.Close();
            _window = null;
        });
    }

    public async Task SaveConfigsAsync(int x, int y, double width, double height, double fontSize, Color color,
        int disappearTimeMs, int animationTimeMs)
    {
        await _localSettingsService.SaveSettingAsync(ConfigStr,
            new TransparentInfoWindowConfig(
                x,
                y,
                width,
                height,
                color.ToUInt32(),
                fontSize,
                disappearTimeMs,
                animationTimeMs
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
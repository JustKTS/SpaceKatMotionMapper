using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using Timer = System.Timers.Timer;

namespace SpaceKatMotionMapper.Services;

public class TransparentInfoService
{
    public bool IsTransparentInfoEnable { get; set; } = true;
    private bool _isWindowShow;

    private readonly Timer _timer = new()
    {
        AutoReset = false,
        Interval = 1500
    };

    private readonly TransparentInfoWindow _transparentInfoWindow;
    private readonly TransparentInfoViewModel _transparentInfoViewModel;
    private readonly ILocalSettingsService _localSettingsService;

    public TransparentInfoService(
        TransparentInfoWindow transparentInfoWindow,
        TransparentInfoViewModel transparentInfoViewModel,
        ILocalSettingsService localSettingsService)
    {
        _transparentInfoWindow = transparentInfoWindow;
        _transparentInfoViewModel = transparentInfoViewModel;
        _localSettingsService = localSettingsService;

        _timer.Elapsed += (sender, args) =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                transparentInfoWindow.SetOpacity(0);
                await Task.Delay(250);
                transparentInfoWindow.Hide();
            });
            _isWindowShow = false;
            // _timer.Enabled = false;
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
            _transparentInfoViewModel.KatAction = action;
            _transparentInfoViewModel.IsOtherInfo = false;
        });
        UpdateWindowState();
    }

    public void DisplayOtherInfo(string info)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _transparentInfoViewModel.OtherInfo = info;
            _transparentInfoViewModel.IsOtherInfo = true;
        });
        UpdateWindowState();
    }

    private void UpdateWindowState()
    {
        if (!IsTransparentInfoEnable) return;

        if (_transparentInfoViewModel.IsAdjustMode) return;

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
            _transparentInfoViewModel.IsAdjustMode = true;
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
            var position = _transparentInfoWindow.Position;

            _localSettingsService.SaveSettingAsync("TransparentInfoWindowConfig",
                new TransparentInfoWindowConfig(
                    position.X,
                    position.Y,
                    _transparentInfoWindow.Width,
                    _transparentInfoWindow.Height
                ));
            _transparentInfoWindow.IsHitTestVisible = false;
            _transparentInfoWindow.IsManagedResizerVisible = false;
            _transparentInfoWindow.ShowActivated = false;
            _transparentInfoViewModel.IsAdjustMode = false;
            _transparentInfoWindow.SetOpacity(0);
            _transparentInfoWindow.Hide();
        });
    }
}
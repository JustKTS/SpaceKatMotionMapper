using System;
using System.Threading.Tasks;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Services;

// ReSharper disable once InconsistentNaming
public class TimeAndDeadZoneVMService(
    MotionTimeConfigViewModel motionTimeConfigViewModel,
    DeadZoneConfigViewModel deadZoneConfigViewModel,
    TimeAndDeadZoneSettingViewModel timeAndDeadZoneSettingViewModel)
{
    public void UpdateByDefault()
    {
        timeAndDeadZoneSettingViewModel.UpdateByDefault();
        motionTimeConfigViewModel.UpdateByDefault();
        motionTimeConfigViewModel.LoadMotionTimeConfigsCommand.Execute(null);
        deadZoneConfigViewModel.UpdateByDefault();
        deadZoneConfigViewModel.LoadDeadZoneAsyncCommand.Execute(null);
    }

    public void UpdateByGuid(Guid id)
    {
        timeAndDeadZoneSettingViewModel.UpdateById(id);
        motionTimeConfigViewModel.UpdateById(id);
        motionTimeConfigViewModel.LoadMotionTimeConfigsCommand.Execute(null);
        deadZoneConfigViewModel.UpdateById(id);
        deadZoneConfigViewModel.LoadDeadZoneAsyncCommand.Execute(null);
    }

    private readonly OverlayDialogOptions _options = new()
    {
        FullScreen = false,
        HorizontalAnchor = HorizontalPosition.Center,
        VerticalAnchor = VerticalPosition.Center,
        HorizontalOffset = 0.0,
        VerticalOffset = 0.0,
        Mode = DialogMode.None,
        Buttons = DialogButton.None,
        Title = "触发时间与死区设置",
        CanLightDismiss = true,
        CanDragMove = true,
        IsCloseButtonVisible = true,
        CanResize = true
    };

    public async Task ShowDialogAsync()
    {
        timeAndDeadZoneSettingViewModel.StartKatListening();
        deadZoneConfigViewModel.StartAxesDataDisplay();
        await OverlayDialog.ShowModal<TimeAndDeadZoneSettingView, TimeAndDeadZoneSettingViewModel>(
            timeAndDeadZoneSettingViewModel, MainWindow.LocalHost, options: _options);

        timeAndDeadZoneSettingViewModel.StopKatListening();
        deadZoneConfigViewModel.StopAxesDataDisplay();
    }
}
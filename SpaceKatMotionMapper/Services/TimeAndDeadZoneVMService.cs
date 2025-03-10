using System;
using System.Threading.Tasks;
using Avalonia.Controls;
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

    private readonly DialogOptions _options = new()
    {
        Mode = DialogMode.None,
        StartupLocation = WindowStartupLocation.CenterScreen,
        ShowInTaskBar = true,
        CanDragMove = true,
        IsCloseButtonVisible = true,
        CanResize = true
    };

    public async Task ShowDialogAsync()
    {
        timeAndDeadZoneSettingViewModel.StartKatListening();
        deadZoneConfigViewModel.StartAxesDataDisplay();
        await Dialog.ShowCustomModal<TimeAndDeadZoneSettingView, TimeAndDeadZoneSettingViewModel, object>(
            timeAndDeadZoneSettingViewModel, options: _options);
        timeAndDeadZoneSettingViewModel.StopKatListening();
        deadZoneConfigViewModel.StopAxesDataDisplay();
    }
}
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Views;
using SpaceKatMotionMapper.States;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;
using Win32Helpers;

namespace SpaceKatMotionMapper.ViewModels;

public partial class AutoDisableViewModel(AutoDisableService autoDisableService) : ViewModelBase
{
    [ObservableProperty] private bool _isEnable;

    partial void OnIsEnableChanged(bool value)
    {
        if (value)
        {
            autoDisableService.IsCurrentFpInList += CurrentFpInListHandler;
        }
        else
        {
            autoDisableService.IsCurrentFpInList -= CurrentFpInListHandler;
        }

        autoDisableService.IsEnable = value;
    }

    private void CurrentFpInListHandler(object? sender, bool e)
    {
        Dispatcher.UIThread.Invoke(() => { App.GetRequiredService<GlobalStates>().IsMapperEnable = e; });
    }

    public ObservableCollection<AutoDisableProgramViewModel> AutoDisableInfos { get; } = [];


    [RelayCommand]
    private async Task OpenRunningProgramSelector()
    {
        var ret = await Dialog.ShowCustomModal<RunningProgramSelector, RunningProgramSelectorViewModel, object?>(
            App.GetRequiredService<RunningProgramSelectorViewModel>(), null, RunningProgramSelectorViewModel.DialogOptions);
        if (ret is not ForeProgramInfo info) return;
        Add(info);
    }

    private void Add(ForeProgramInfo info)
    {
        if (autoDisableService.IsPathContained(info.ProcessFileAddress)) return;
        AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, info.ProcessFileAddress));
        App.GetRequiredService<AutoDisableService>().AddProgramPath(info.ProcessFileAddress);
    }

    public void LoadInfos()
    {
        autoDisableService.WaitForInitializedAsync().ContinueWith(t =>
        {
            if (!t.Result) return;
            IsEnable = autoDisableService.IsEnable;
            AutoDisableInfos.Clear();
            autoDisableService.GetAllProgramPaths()
                .Iter(e => AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, e)));
        });
    }
}

public partial class AutoDisableProgramViewModel(AutoDisableViewModel parent, string programProgramPath) : ViewModelBase
{
    public string ProgramPath { get; } = programProgramPath;
    public string Name => Path.GetFileNameWithoutExtension(ProgramPath);

    [RelayCommand]
    private void RemoveSelf()
    {
        parent.AutoDisableInfos.Remove(this);
        App.GetRequiredService<AutoDisableService>().RemoveProgramPath(ProgramPath);
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatMotionMapper.Services;
using Win32Helpers;

namespace SpaceKatMotionMapper.ViewModels;

public partial class AutoDisableViewModel(AutoDisableService autoDisableService) : ViewModelBase
{
    [ObservableProperty] private bool _isEnable;

    partial void OnIsEnableChanged(bool value)
    {
        autoDisableService.IsEnable = value;
    }

    public ObservableCollection<AutoDisableProgramViewModel> AutoDisableInfos { get; } = [];

    public void Add(ForeProgramInfo info)
    {
        if (autoDisableService.IsPathContained(info.ProcessFileAddress)) return;
        AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, info.ProcessFileAddress));
        App.GetRequiredService<AutoDisableService>().AddProgramPath(info.ProcessFileAddress);
    }

    public void LoadInfos()
    {
        IsEnable = autoDisableService.IsEnable;
        AutoDisableInfos.Clear();
        autoDisableService.GetAllProgramPaths()
            .Iter(e => AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, e)));
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
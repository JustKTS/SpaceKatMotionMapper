using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Views;
using Ursa.Controls;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.ViewModels;

public partial class AutoDisableViewModel : ViewModelBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AutoDisableViewModel>();

    private readonly IAutoDisableService _autoDisableService;
    private readonly IGlobalStates _globalStates;
    private readonly RunningProgramSelectorViewModel _runningProgramSelectorVM;

    public AutoDisableViewModel(
        IAutoDisableService autoDisableService,
        IGlobalStates globalStates,
        RunningProgramSelectorViewModel runningProgramSelectorVM)
    {
        _autoDisableService = autoDisableService;
        _globalStates = globalStates;
        _runningProgramSelectorVM = runningProgramSelectorVM;
    }

    [ObservableProperty] private bool _isEnable;
    [ObservableProperty] private bool _isPlatformSupported;

    partial void OnIsEnableChanged(bool value)
    {
        if (value && !IsPlatformSupported)
        {
            Log.Warning("Cannot enable: platform not supported, forcing to false");
            IsEnable = false;
            return;
        }

        if (value)
        {
            _autoDisableService.IsCurrentFpInList += CurrentFpInListHandler;
        }
        else
        {
            _autoDisableService.IsCurrentFpInList -= CurrentFpInListHandler;
        }

        _autoDisableService.IsEnable = value;
    }

    private void CurrentFpInListHandler(object? sender, bool e)
    {
        Dispatcher.UIThread.Invoke(() => { _globalStates.IsMapperEnable = e; });
    }

    public ObservableCollection<AutoDisableProgramViewModel> AutoDisableInfos { get; } = [];


    [RelayCommand]
    private async Task OpenRunningProgramSelector()
    {
        var ret = await Dialog.ShowCustomAsync<RunningProgramSelector, RunningProgramSelectorViewModel, object?>(
            _runningProgramSelectorVM, null, RunningProgramSelectorViewModel.DialogOptions);
        if (ret is not ForeProgramInfo info) return;
        Add(info);
    }

    private void Add(ForeProgramInfo info)
    {
        if (_autoDisableService.IsPathContained(info.ProcessFileAddress)) return;
        AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, info.ProcessFileAddress, info.ProcessName, _autoDisableService));
        _autoDisableService.AddProgramPath(info.ProcessFileAddress, info.ProcessName);
    }

    public void LoadInfos()
    {
        _autoDisableService.WaitForInitializedAsync().ContinueWith(t =>
        {
            if (!t.Result) return;

            IsPlatformSupported = _autoDisableService.IsPlatformSupported;

            if (IsPlatformSupported)
            {
                IsEnable = _autoDisableService.IsEnable;
            }
            else
            {
                IsEnable = false;
            }

            AutoDisableInfos.Clear();
            _autoDisableService.GetAllProgramPaths()
                .Iter(e => AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, e, string.Empty, _autoDisableService)));
        });
    }
}

public partial class AutoDisableProgramViewModel : ViewModelBase
{
    private readonly AutoDisableViewModel _parent;
    private readonly IAutoDisableService _autoDisableService;
    public string ProgramPath { get; }
    public string ProcessName { get; }

    public AutoDisableProgramViewModel(AutoDisableViewModel parent, string programProgramPath, string processName, IAutoDisableService autoDisableService)
    {
        _parent = parent;
        _autoDisableService = autoDisableService;
        ProgramPath = programProgramPath;
        ProcessName = processName;
    }

    public string Name
    {
        get
        {
            if (!string.IsNullOrEmpty(ProgramPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(ProgramPath);
                if (!string.IsNullOrEmpty(fileName))
                {
                    return fileName;
                }
            }

            if (!string.IsNullOrEmpty(ProcessName))
            {
                return ProcessName;
            }

            return "Unknown Program";
        }
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        _parent.AutoDisableInfos.Remove(this);
        _autoDisableService.RemoveProgramPath(ProgramPath);
    }
}
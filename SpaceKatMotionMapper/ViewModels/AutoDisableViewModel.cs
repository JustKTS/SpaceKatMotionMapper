using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Views;
using SpaceKatMotionMapper.States;
using Ursa.Controls;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.ViewModels;

public partial class AutoDisableViewModel(AutoDisableService autoDisableService) : ViewModelBase
{
    [ObservableProperty] private bool _isEnable;
    [ObservableProperty] private bool _isPlatformSupported;

    partial void OnIsEnableChanged(bool value)
    {
        // 双重检查：如果平台不支持，坚决阻止启用
        if (value && !IsPlatformSupported)
        {
            Log.Warning("[{ViewModel}] Cannot enable: platform not supported, forcing to false", nameof(AutoDisableViewModel));
            IsEnable = false;
            return;
        }

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
        var ret = await Dialog.ShowCustomAsync<RunningProgramSelector, RunningProgramSelectorViewModel, object?>(
            App.GetRequiredService<RunningProgramSelectorViewModel>(), null, RunningProgramSelectorViewModel.DialogOptions);
        if (ret is not ForeProgramInfo info) return;
        Add(info);
    }

    private void Add(ForeProgramInfo info)
    {
        if (autoDisableService.IsPathContained(info.ProcessFileAddress)) return;
        AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, info.ProcessFileAddress, info.ProcessName));
        App.GetRequiredService<AutoDisableService>().AddProgramPath(info.ProcessFileAddress, info.ProcessName);
    }

    public void LoadInfos()
    {
        autoDisableService.WaitForInitializedAsync().ContinueWith(t =>
        {
            if (!t.Result) return;

            // 加载平台支持状态
            IsPlatformSupported = autoDisableService.IsPlatformSupported;

            if (IsPlatformSupported)
            {
                IsEnable = autoDisableService.IsEnable;
            }
            else
            {
                IsEnable = false;
            }

            // 始终加载已保存的程序列表（即使平台不支持，也显示列表）
            AutoDisableInfos.Clear();
            autoDisableService.GetAllProgramPaths()
                .Iter(e => AutoDisableInfos.Add(new AutoDisableProgramViewModel(this, e, string.Empty)));
        });
    }
}

public partial class AutoDisableProgramViewModel(AutoDisableViewModel parent, string programProgramPath, string processName) : ViewModelBase
{
    public string ProgramPath { get; } = programProgramPath;
    public string ProcessName { get; } = processName;

    public string Name
    {
        get
        {
            // 优先从路径中提取名称
            if (!string.IsNullOrEmpty(ProgramPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(ProgramPath);
                if (!string.IsNullOrEmpty(fileName))
                {
                    return fileName;
                }
            }

            // 降级到进程名
            if (!string.IsNullOrEmpty(ProcessName))
            {
                return ProcessName;
            }

            // 最后的备选
            return "Unknown Program";
        }
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        parent.AutoDisableInfos.Remove(this);
        App.GetRequiredService<AutoDisableService>().RemoveProgramPath(ProgramPath);
    }
}
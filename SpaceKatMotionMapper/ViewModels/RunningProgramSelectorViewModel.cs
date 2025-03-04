using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Services.Contract;
using Win32Helpers;

namespace SpaceKatMotionMapper.ViewModels;

public partial class RunningProgramSelectorViewModel(AutoDisableViewModel parent) : ViewModelBase
{
#if DEBUG
    public RunningProgramSelectorViewModel() : this(null!)
    {
    }
#endif

    public ObservableCollection<RunningProgramSelectorSubViewModel> ForeProcessInfos { get; } = [];

    public void UpdateForeProcessInfos()
    {
        ForeProcessInfos.Clear();
        var fpInfos = CurrentForeProgramHelper.FindAll();
        fpInfos.Select(info => new RunningProgramSelectorSubViewModel(this, info)).Iter(ForeProcessInfos.Add);
    }

    public void AddToAutoDisable(ForeProgramInfo info)
    {
        parent.Add(info);
    }
    
    [RelayCommand]
    private async Task SelectProcess()
    {
        var storageProvider =
            App.GetRequiredService<IStorageProviderService>().GetStorageProvider();

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypeDefines.Exe], Title = "程序文件" });
        if (files.Count == 0)
        {
            return;
        }

        var file = files[0];
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            parent.Add(new ForeProgramInfo(file.Name, file.Name, file.Name, file.Path.LocalPath));
        });
    }
}

public partial class RunningProgramSelectorSubViewModel(RunningProgramSelectorViewModel parent, ForeProgramInfo info)
    : ViewModelBase
{
    public ForeProgramInfo Info => info;
    [RelayCommand]
    private void Add()
    {
        parent.AddToAutoDisable(info);
    }
}
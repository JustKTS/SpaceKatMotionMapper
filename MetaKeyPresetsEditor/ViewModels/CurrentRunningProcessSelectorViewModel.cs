using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetaKeyPresetsEditor.Helpers;
using SpaceKat.Shared.Helpers;
using MetaKeyPresetsEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using SpaceKat.Shared.Defines;
using PlatformAbstractions;

namespace MetaKeyPresetsEditor.ViewModels;

public partial class CurrentRunningProcessSelectorViewModel(IPlatformWindowService platformWindowService)
    : ViewModelBase
{
    public ObservableCollection<ForeProgramInfo> ForeProcessInfos { get; } = [];

    [ObservableProperty] private ForeProgramInfo? _selectedFpInfo;
    public bool IsWindowsPlatform => PlatformHelper.IsWindows();

    public async Task UpdateForeProcessInfosAsync()
    {
        ForeProcessInfos.Clear();
        var fpInfos = await Task.Run(platformWindowService.FindAllForegroundPrograms);
        Dispatcher.UIThread.Invoke(() => { fpInfos.Iter(ForeProcessInfos.Add); });
    }

    partial void OnSelectedFpInfoChanged(ForeProgramInfo? value)
    {
        if (value is null) return;
        DIHelper.GetServiceProvider().GetRequiredService<IUiInteractService>()
            .ChangeConfigNameAsync(Path.GetFileNameWithoutExtension(value.ProcessFileAddress));
    }

    [RelayCommand]
    private static async Task SelectFromFile()
    {
        var storageFiles = await DIHelper.GetServiceProvider().GetRequiredService<IStorageProvider>().OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false, FileTypeFilter = [FilePickerFileTypeDefines.Exe],
                Title = "请选择可执行文件"
            });
        if (storageFiles.Count == 0) return;
        var file = storageFiles[0];
        await DIHelper.GetServiceProvider().GetRequiredService<IUiInteractService>()
            .ChangeConfigNameAsync(Path.GetFileNameWithoutExtension(file.Path.LocalPath));
    }
}
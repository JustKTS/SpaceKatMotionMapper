using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using PlatformAbstractions;
using Serilog;
using SpaceKat.Shared.Defines;
using SpaceKat.Shared.Services.Contract;
using Ursa.Controls;

namespace SpaceKat.Shared.ViewModels;

public partial class RunningProgramSelectorViewModel : ObservableObject, IDialogContext
{
    public static readonly DialogOptions DialogOptions = new()
    {
        Button = DialogButton.None,
        CanDragMove = false,
        CanResize = true,
        IsCloseButtonVisible = true,
        Mode = DialogMode.None
    };

    public ObservableCollection<ForeProgramInfo> ForeProcessInfos { get; } = [];
    [ObservableProperty] private ForeProgramInfo? _selectedForeProcessInfo;

    partial void OnSelectedForeProcessInfoChanged(ForeProgramInfo? value)
    {
        if (value is null) return;
        ReturnFpInfo(value);
    }

    private readonly IStorageProviderService _storageProviderService;
    private readonly IPlatformWindowService _platformWindowService;

    public RunningProgramSelectorViewModel(IStorageProviderService storageProviderService, IPlatformWindowService platformWindowService)
    {
        _storageProviderService = storageProviderService;
        _platformWindowService = platformWindowService;
        Log.Debug("[{ViewModel}] Constructor called, service type: {ServiceType}",
            nameof(RunningProgramSelectorViewModel), _platformWindowService.GetType().FullName);
        // 不在构造函数中加载数据，等待对话框显示时再加载
    }

    /// <summary>
    /// 更新前台程序列表（在对话框显示时调用）
    /// </summary>
    public async Task UpdateForeProcessInfosAsync(CancellationToken cancellationToken=default)
    {
        Log.Debug("[{ViewModel}] UpdateForeProcessInfosAsync() started", nameof(RunningProgramSelectorViewModel));
        ForeProcessInfos.Clear();
        var count = 0;
        await foreach(var fpInfo in _platformWindowService.FindAllForegroundProgramsAsync(cancellationToken))
        {
            count++;
            Log.Debug("[{ViewModel}] Received window info #{Count}: {Title}",
                nameof(RunningProgramSelectorViewModel), count, fpInfo.Title);
            Dispatcher.UIThread.Invoke(()=>
            {
                ForeProcessInfos.Add(fpInfo);
            });
        }
        Log.Information("[{ViewModel}] UpdateForeProcessInfosAsync() completed, total windows: {Count}",
            nameof(RunningProgramSelectorViewModel), count);
    }


    [RelayCommand]
    private async Task SelectProcess()
    {
        var storageProvider = _storageProviderService.GetStorageProvider();

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypeDefines.Exe], Title = "程序文件" });
        if (files.Count == 0)
        {
            return;
        }

        var file = files[0];
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            RequestClose?.Invoke(this, new ForeProgramInfo(file.Name, file.Name, file.Name, file.Path.LocalPath));
        });
    }

    public async void ReturnFpInfo(ForeProgramInfo info)
    {
        await Dispatcher.UIThread.InvokeAsync(() => RequestClose?.Invoke(this, info));
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}
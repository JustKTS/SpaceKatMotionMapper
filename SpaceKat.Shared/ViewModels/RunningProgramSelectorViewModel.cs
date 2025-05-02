using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using SpaceKat.Shared.Defines;
using SpaceKat.Shared.Services.Contract;
using Ursa.Controls;
using Win32Helpers;

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

    public RunningProgramSelectorViewModel(IStorageProviderService storageProviderService)
    {
        _storageProviderService = storageProviderService;
        UpdateForeProcessInfos();
    }

    private void UpdateForeProcessInfos()
    {
        ForeProcessInfos.Clear();
        var fpInfos = CurrentForeProgramHelper.FindAll();
        fpInfos.Iter(ForeProcessInfos.Add);
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

    public void ReturnFpInfo(ForeProgramInfo info)
    {
        RequestClose?.Invoke(this, info);
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}
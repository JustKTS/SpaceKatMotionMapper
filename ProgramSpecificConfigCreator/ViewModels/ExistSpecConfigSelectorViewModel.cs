using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using ProgramSpecificConfigCreator.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace ProgramSpecificConfigCreator.ViewModels;

public partial class ExistSpecConfigSelectorViewModel : ObservableRecipient, IDialogContext
{
    private readonly IProgramSpecMetaKeyFileService _metaKeyFileService =
        DIHelper.GetServiceProvider().GetRequiredService<IProgramSpecMetaKeyFileService>();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ProgramNames))]
    private Dictionary<string, ProgramSpecMetaKeysRecord> _programSpecificConfigs = [];

    [ObservableProperty] private string _selectedConfigName = string.Empty;

    public string[] ProgramNames => ProgramSpecificConfigs.Keys.ToArray();

    public ExistSpecConfigSelectorViewModel()
    {
        _ = LoadConfigs();
    }

    [RelayCommand]
    private async Task LoadConfigs()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dict = _metaKeyFileService.LoadConfigs();
            dict.Remove("默认配置");
            ProgramSpecificConfigs = dict;
        });
    }


    partial void OnSelectedConfigNameChanged(string value)
    {
        Close();
        if (!ProgramSpecificConfigs.TryGetValue(value, out var record)) return;
        _ = ChangeMetaKeyVMHelper.LoadVMFromConfig(record);
    }


    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}
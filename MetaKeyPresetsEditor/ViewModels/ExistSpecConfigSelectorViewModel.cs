using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using LanguageExt;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace MetaKeyPresetsEditor.ViewModels;

public partial class ExistSpecConfigSelectorViewModel : ObservableRecipient, IDialogContext
{
    private readonly IMetaKeyPresetFileService _metaKeyPresetFileService =
        DIHelper.GetServiceProvider().GetRequiredService<IMetaKeyPresetFileService>();

    [ObservableProperty] private Dictionary<string, ProgramSpecMetaKeysRecord> _programSpecificConfigs = [];

    [ObservableProperty] private ProgramSpecMetaKeysRecord _selectedConfig =
        new ProgramSpecMetaKeysRecord(string.Empty, string.Empty, false, [], []);

    public ExistSpecConfigSelectorViewModel()
    {
        _ = LoadConfigs();
    }

    [RelayCommand]
    private async Task LoadConfigs()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var ret = _metaKeyPresetFileService.LoadConfigs();
            ret.Match(dict =>
                {
                    dict.Remove("默认配置");
                    ProgramSpecificConfigs = dict;
                    OnFilterStrChanged(string.Empty);
                }, ex =>
                {
                    DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationSpecService>()
                        .ShowPopUpNotificationAsync(new PopupNotificationData(NotificationType.Error, ex.Message));
                }
            );
        });
    }


    partial void OnSelectedConfigChanged(ProgramSpecMetaKeysRecord value)
    {
        Close();
        _ = ChangeMetaKeyVMHelper.LoadVMFromConfig(value);
    }

    #region 筛选

    public ObservableCollection<ProgramSpecMetaKeysRecord> ConfigsFiltered { get; } = [];


    [ObservableProperty] private string _filterStr = string.Empty;

    partial void OnFilterStrChanged(string value)
    {
        ConfigsFiltered.Clear();

        if (string.IsNullOrEmpty(value))
        {
            ProgramSpecificConfigs.Iter(e => ConfigsFiltered.Add(e.Value));
            return;
        }

        ProgramSpecificConfigs.Where(e => e.Key.ToLower().Contains(value.ToLower()))
            .Iter(e => ConfigsFiltered.Add(e.Value));
    }

    #endregion


    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}
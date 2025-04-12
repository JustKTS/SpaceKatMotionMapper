using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Irihi.Avalonia.Shared.Contracts;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Services;

namespace SpaceKatMotionMapper.ViewModels;

public partial class ProgramSpecMetaKeySelectorViewModel : ViewModelBase, IDialogContext
{
    private readonly ProgramSpecMetaKeyService _programSpecMetaKeyService =
        App.GetRequiredService<ProgramSpecMetaKeyService>();

    [ObservableProperty] private List<ProgramSpecMetaKeySelectorSubViewModel> _configs = [];

    public readonly KeyActionConfigViewModel Parent;

    public ProgramSpecMetaKeySelectorViewModel(KeyActionConfigViewModel parent)
    {
        Parent = parent;
        LoadConfig();
    }

    private void LoadConfig()
    {
        Configs = _programSpecMetaKeyService.Configs
            .Where(x =>
                x.Key == "默认配置" ||
                x.Key == Path.GetFileNameWithoutExtension(Parent.Parent.Parent.Parent.ProcessFilename))
            .Select(x => new ProgramSpecMetaKeySelectorSubViewModel(this, x.Value)).ToList();
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}

public partial class ProgramSpecMetaKeySelectorSubViewModel(
    ProgramSpecMetaKeySelectorViewModel parent,
    ProgramSpecMetaKeysRecord? record) : ViewModelBase
{
    public string[] Descriptions => record is null
        ? []
        : record.CombinationKeys.Keys.Concat(record.MacroKeys.Keys).ToArray();
    public string[] Contributors => record is null?[]:record.Contributors.Split(";");
    public string ProgramName => record is null ? string.Empty : record.ConfigName;

    #region 点击添加

    [ObservableProperty] private string _selectedDescription = string.Empty;

    partial void OnSelectedDescriptionChanged(string? value)
    {
        if (record is null) return;
        if (value is null) return;
        if (record.CombinationKeys.TryGetValue(value, out var keys))
        {
            parent.Parent.AddHotKeyActions(keys.UseCtrl, keys.UseWin, keys.UseAlt, keys.UseShift, keys.Key, value);
            parent.Close();
            return;
        }

        if (!record.MacroKeys.TryGetValue(value, out var keyActionConfigs)) return;
        parent.Parent.AddCustomActions(value, keyActionConfigs);
        parent.Close();


    }

    #endregion
}
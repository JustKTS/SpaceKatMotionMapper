using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using WindowsInput;

namespace SpaceKatMotionMapper.ViewModels;

public partial class MetaKeyPresetSelectorViewModel : ViewModelBase, IDialogContext
{
    private readonly MetaKeyPresetService _metaKeyPresetService =
        App.GetRequiredService<MetaKeyPresetService>();

    [ObservableProperty] private List<MetaKeySelectorSubViewModel> _configs = [];

    private readonly KeyActionConfigViewModel _parent;
    private IRelayCommand<KeyActionsForPresetRecord> AddCustomActionsCommand { get; }
    private IRelayCommand<KeyValuePair<string, CombinationKeysRecord>> AddHotKeyActionsCommand{ get; }

    public MetaKeyPresetSelectorViewModel(
        KeyActionConfigViewModel parent,
        IRelayCommand<KeyActionsForPresetRecord> addCustomActionsCommand,
        IRelayCommand<KeyValuePair<string, CombinationKeysRecord>> addHotKeyActionsCommand)
    {
        _parent = parent;
        AddCustomActionsCommand = addCustomActionsCommand;
        AddHotKeyActionsCommand = addHotKeyActionsCommand;
        LoadConfig();
    }

    private void LoadConfig()
    {
        _metaKeyPresetService.ReloadConfigs();
        Configs = _metaKeyPresetService.Configs
            .Where(x =>
                x.Value.IsGeneral ||
                x.Key == Path.GetFileNameWithoutExtension(_parent.Parent.Parent.Parent.ProcessFilename))
            .Select(x => new MetaKeySelectorSubViewModel(x.Value,
                new RelayCommand(Close),
               AddCustomActionsCommand,
                AddHotKeyActionsCommand
                )).ToList();
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}

public partial class MetaKeySelectorSubViewModel : ViewModelBase
{
    public string[] Contributors { get; }
    public string ConfigName { get; }
    
    public PresetKeyActionDisplay[] DisplayActions { get; }
    
    private readonly ProgramSpecMetaKeysRecord? _record;

    private IRelayCommand CloseParentCommand { get; }
    private IRelayCommand<KeyActionsForPresetRecord> AddCustomActionsCommand { get; }
    private IRelayCommand<KeyValuePair<string, CombinationKeysRecord>> AddHotKeyActionsCommand{ get; }
    
    public MetaKeySelectorSubViewModel(
        ProgramSpecMetaKeysRecord? record,
        IRelayCommand closeParentCommand,
        IRelayCommand<KeyActionsForPresetRecord> addCustomActionsCommand,
        IRelayCommand<KeyValuePair<string, CombinationKeysRecord>> addHotKeyActionsCommand)
    {
        _record = record;
        CloseParentCommand = closeParentCommand;
        AddCustomActionsCommand = addCustomActionsCommand;
        AddHotKeyActionsCommand = addHotKeyActionsCommand;
        
        Contributors = record is null ? [] : record.Contributors.Split(";");
        ConfigName = record is null ? string.Empty : record.ConfigName;

        if (record is null)
        {
            DisplayActions = [];
            return;
        }
        
        var displayRecordsPart1 = record.CombinationKeys.Select(kv =>
        {
            var (key, value) = kv;
            List<KeyActionConfig> actions = [];
            if (value.UseCtrl)
                actions.Add(new KeyActionConfig(ActionType.KeyBoard, "CTRL", PressModeEnum.Press, 1));
            if (value.UseAlt)
                actions.Add(new KeyActionConfig(ActionType.KeyBoard, "ALT", PressModeEnum.Press, 1));
            if (value.UseShift)
                actions.Add(new KeyActionConfig(ActionType.KeyBoard, "SHIFT", PressModeEnum.Press, 1));
            if (value.UseWin)
                actions.Add(new KeyActionConfig(ActionType.KeyBoard, "WIN", PressModeEnum.Press, 1));
            
            actions.Add(new KeyActionConfig(ActionType.KeyBoard, value.Key.GetWrappedName(), PressModeEnum.Click, 1));
            return new PresetKeyActionDisplay(key, actions.ToArray());
        });
        var displayRecordsPart2 = record.MacroKeys.Select(kv =>
        {
            var (key, value) = kv;
            var actions = KatMotionConfigDisplayHelper.GenerateDisplayList(value);
            return new PresetKeyActionDisplay(key, actions);
        });
        DisplayActions = displayRecordsPart1.Concat(displayRecordsPart2).ToArray();
    }


    #region 点击添加

    [ObservableProperty] private PresetKeyActionDisplay? _selectedMetaKey = null;

    partial void OnSelectedMetaKeyChanged(PresetKeyActionDisplay? value)
    {
        if (_record is null) return;
        if (value is null) return;
        if (_record.CombinationKeys.TryGetValue(value.Description, out var keys))
        {
            AddHotKeyActionsCommand.Execute(new KeyValuePair<string, CombinationKeysRecord>(value.Description, keys));
            CloseParentCommand.Execute(null);
            return;
        }

        if (!_record.MacroKeys.TryGetValue(value.Description, out var keyActionConfigs)) return;
        
        AddCustomActionsCommand.Execute(new KeyActionsForPresetRecord(value.Description, keyActionConfigs));
        CloseParentCommand.Execute(null);
    }

    #endregion
}

public record PresetKeyActionDisplay(string Description, KeyActionConfig[] Actions);
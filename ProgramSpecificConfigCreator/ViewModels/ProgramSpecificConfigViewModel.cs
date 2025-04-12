using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using ProgramSpecificConfigCreator.Helpers;
using ProgramSpecificConfigCreator.Services;
using Serilog;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace ProgramSpecificConfigCreator.ViewModels;

public partial class ProgramSpecificConfigViewModel : ViewModelBase
{
    private readonly ILogger _logger = DIHelper.GetServiceProvider().GetRequiredService<ILogger>();

    [ObservableProperty] private bool _isDefault;
    [ObservableProperty] private string _configName = string.Empty;
    [ObservableProperty] private bool _isGeneral;

    [ObservableProperty] private bool _isConfigNameEditing;
    public ObservableCollection<string> Contributors { get; } = [];

    partial void OnIsDefaultChanged(bool value)
    {
        if (!value) return;
        ConfigName = "默认配置";
        IsGeneral = true;
    }
    
    partial void OnIsGeneralChanged(bool value)
    {
        ConfigName = string.Empty;
    }


    public async Task ChangeProgramNameAsync(string programName)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { ConfigName = programName; });
    }


    # region 配置管理

    public ObservableCollection<CombinationKeysConfigViewModel> CombinationKeysConfigs { get; } = [];
    public ObservableCollection<CombinationKeysConfigViewModel> CombinationKeysConfigsFiltered { get; } = [];


    [ObservableProperty] private bool _isCombinationKeyFilter;

    partial void OnIsCombinationKeyFilterChanged(bool value)
    {
        if (value) OnCombinationKeysFilterStrChanged(CombinationKeysFilterStr);
        else CombinationKeysFilterStr = string.Empty;
    }

    [ObservableProperty] private string _combinationKeysFilterStr = string.Empty;

    partial void OnCombinationKeysFilterStrChanged(string value)
    {
        CombinationKeysConfigsFiltered.Clear();

        if (string.IsNullOrEmpty(value))
        {
            CombinationKeysConfigs.Iter(CombinationKeysConfigsFiltered.Add);
            return;
        }

        CombinationKeysConfigs.Where(vm => vm.Description.Contains(value))
            .Iter(CombinationKeysConfigsFiltered.Add);
    }

    public ObservableCollection<KeyActionConfigViewModel> KeyActionConfigs { get; } = [];
    public ObservableCollection<KeyActionConfigViewModel> KeyActionConfigsFiltered { get; } = [];

    [ObservableProperty] private bool _isKeyActionFilter;

    partial void OnIsKeyActionFilterChanged(bool value)
    {
        if (value) OnKeyActionFilterStrChanged(CombinationKeysFilterStr);
        else
        {
            KeyActionFilterStr = string.Empty;
        }
    }

    [ObservableProperty] private string _keyActionFilterStr = string.Empty;

    partial void OnKeyActionFilterStrChanged(string value)
    {
        KeyActionConfigsFiltered.Clear();

        if (string.IsNullOrEmpty(value))
        {
            KeyActionConfigs.Iter(KeyActionConfigsFiltered.Add);
            return;
        }

        KeyActionConfigs.Where(vm => vm.Description.Contains(value))
            .Iter(KeyActionConfigsFiltered.Add);
    }

    public void ClearAll()
    {
        IsDefault = false;
        ConfigName = string.Empty;
        IsKeyActionFilter = false;
        IsCombinationKeyFilter = false;
        CombinationKeysConfigs.Clear();
        KeyActionConfigs.Clear();
        Contributors.Clear();
    }

    [RelayCommand]
    private void AddCombinationKeysConfig()
    {
        IsCombinationKeyFilter = false;
        CombinationKeysConfigs.Add(new CombinationKeysConfigViewModel() { Parent = CombinationKeysConfigs });
    }

    [RelayCommand]
    private void AddKeyActionConfig()
    {
        IsKeyActionFilter = false;
        KeyActionConfigs.Add(new KeyActionConfigViewModel() { Parent = KeyActionConfigs });
    }

    private Result<bool> CheckAvailable()
    {
        if (string.IsNullOrEmpty(ConfigName)) return new Result<bool>(new Exception("未指定关联程序，请选择。"));
        var ret = CombinationKeysConfigs.Any(vm =>
            string.IsNullOrEmpty(vm.Description) || string.IsNullOrEmpty(vm.HotKey));
        if (ret) return new Result<bool>(new Exception("组合式快捷键配置出现错误，请检查。"));
        ret = KeyActionConfigs.Any(vm => string.IsNullOrEmpty(vm.Description) || vm.IsAvailable is false);
        return ret ? new Result<bool>(new Exception("宏配置出现错误，请检查。")) : true;
    }

    private ProgramSpecMetaKeysRecord ToConfigRecord()
    {
        Dictionary<string, CombinationKeysRecord> combinationKeys = [];
        CombinationKeysConfigs.Iter(vm => { combinationKeys.Add(vm.Description, vm.ToRecord()); });

        Dictionary<string, List<KeyActionConfig>> macroKeys = [];
        KeyActionConfigs.Iter(vm => { macroKeys.Add(vm.Description, vm.ToKeyActionConfigList()); });
        return new ProgramSpecMetaKeysRecord(ConfigName, string.Join(";", Contributors), IsGeneral, combinationKeys,
            macroKeys);
    }

    # endregion

    #region 保存配置

    private readonly IProgramSpecMetaKeyFileService _metaKeyFileService =
        DIHelper.GetServiceProvider().GetRequiredService<IProgramSpecMetaKeyFileService>();

    [RelayCommand]
    private async Task SaveToConfigDir()
    {
        var ret = await CheckAvailable().MapAsync(async _ =>
        {
            return await Task.Run(() => _metaKeyFileService.SaveToConfigDir(ToConfigRecord()));
        });
        _ = ret.Match(s => s, ex =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationService>().ShowPopUpNotificationAsync(
                new PopupNotificationData(
                    NotificationType.Error,
                    $"保存失败，具体错误信息为：{ex.Message}"));
            return false;
        });
    }

    [RelayCommand]
    private async Task SaveToFile()
    {
        var ret = await CheckAvailable()
            .MapAsync(async _ =>
            {
                FilePickerSaveOptions options = new()
                {
                    Title = "保存配置文件",
                    FileTypeChoices = [new FilePickerFileType("json") { Patterns = ["*.json"] }],
                    SuggestedFileName = $"{Path.GetFileNameWithoutExtension(ConfigName)}.json",
                    DefaultExtension = "json", ShowOverwritePrompt = true
                };

                var sp = DIHelper.GetServiceProvider().GetRequiredService<IStorageProvider>();

                var retFilepath = await sp.SaveFilePickerAsync(options);
                if (retFilepath is null) return false;
                return await Task.Run(() =>
                    _metaKeyFileService.SaveToFile(ToConfigRecord(), retFilepath.Path.LocalPath));
            });

        _ = ret.Match(_ => true, ex =>
        {
            DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationService>().ShowPopUpNotificationAsync(
                new PopupNotificationData(
                    NotificationType.Error,
                    $"保存失败，具体错误信息为：{ex.Message}"));
            return false;
        });
    }

    #endregion

    # region 从Record中读取

    public async Task<Result<bool>> LoadFromRecord(ProgramSpecMetaKeysRecord record)
    {
        IsCombinationKeyFilter = false;
        IsKeyActionFilter = false;

        try
        {
            ConfigName = record.ConfigName;
            IsGeneral = record.IsGeneral;

            var contributors = record.Contributors.Split(";");
            Contributors.Clear();
            contributors.Iter(Contributors.Add);

            IsDefault = ConfigName == "默认配置";
            CombinationKeysConfigs.Clear();
            KeyActionConfigs.Clear();
            CombinationKeysConfigsFiltered.Clear();
            KeyActionConfigsFiltered.Clear();

            foreach (var (k, v) in record.CombinationKeys)
            {
                var vm = new CombinationKeysConfigViewModel
                {
                    Parent = CombinationKeysConfigs,
                    Description = k,
                };
                vm.FromRecord(v);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CombinationKeysConfigs.Add(vm);
                    CombinationKeysConfigsFiltered.Add(vm);
                });
            }

            foreach (var (k, v) in record.MacroKeys)
            {
                var vm = new KeyActionConfigViewModel
                {
                    Parent = KeyActionConfigs,
                    Description = k,
                    IsCustomDescription = true
                };

                vm.FromKeyActionConfig(v);

                await Dispatcher.UIThread.InvokeAsync(() => KeyActionConfigs.Add(vm));
            }


            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "");
            await DIHelper.GetServiceProvider().GetRequiredService<IPopUpNotificationService>()
                .ShowPopUpNotificationAsync(
                    new PopupNotificationData(NotificationType.Error,
                        $"读取配置文件失败，具体错误信息为：{e.Message}"));
            return new Result<bool>(e);
        }
    }

    # endregion
}
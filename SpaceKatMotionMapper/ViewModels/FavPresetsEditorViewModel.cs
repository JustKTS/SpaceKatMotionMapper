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
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using MetaKeyPresetsEditor.Helpers;
using Serilog;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class FavPresetsEditorViewModel : ViewModelBase, IDialogContext
{
    private readonly ILogger _logger;

    public FavPresetsEditorViewModel()
    {
        _logger = App.GetRequiredService<ILogger>();
        LoadFromRecord();
    }

    # region 配置管理

    public ObservableCollection<CombinationKeysConfigViewModel> CombinationKeysConfigs { get; } = [];
    public ObservableCollection<CombinationKeysConfigViewModel> CombinationKeysConfigsFiltered { get; } = [];


    [ObservableProperty] private bool _isCombinationKeyFilter;

    partial void OnIsCombinationKeyFilterChanged(bool value)
    {
        if (value) OnCombinationKeysFilterStrChanged(CombinationKeysFilterStr);
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

    public ObservableCollection<KeyActionConfigForPresetsViewModel> KeyActionConfigs { get; } = [];
    public ObservableCollection<KeyActionConfigForPresetsViewModel> KeyActionConfigsFiltered { get; } = [];

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
        IsKeyActionFilter = false;
        IsCombinationKeyFilter = false;
        CombinationKeysConfigs.Clear();
        KeyActionConfigs.Clear();
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
        KeyActionConfigs.Add(new KeyActionConfigForPresetsViewModel { Parent = KeyActionConfigs });
    }

    private Result<bool> CheckAvailable()
    {
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
        return new ProgramSpecMetaKeysRecord("我的收藏", string.Empty, true, combinationKeys,
            macroKeys);
    }

    # endregion

    # region 数据存取

    private readonly IMetaKeyPresetFileService _metaKeyPresetFileService =
        DIHelper.GetServiceProvider().GetRequiredService<IMetaKeyPresetFileService>();

    [RelayCommand]
    private async Task SaveToConfigDir()
    {
        var ret = await CheckAvailable().MapAsync(async _ =>
        {
            return await Task.Run(() => _metaKeyPresetFileService.SaveToConfigDir(ToConfigRecord()));
        });
        _ = ret.Match(_ =>
        {
            Close();
            return true;
        }, ex =>
        {
            MessageBox.ShowOverlayAsync($"保存失败，{ex.Message}", hostId: FavPresetsEditorView.LocalHost,
                icon: MessageBoxIcon.Warning);
            // App.GetRequiredService<PopUpNotificationService>().Pop(
            //         NotificationType.Error,
            //         $"保存失败，{ex.Message}");
            return false;
        });
    }

    private Result<bool> LoadFromRecord()
    {
        var eitherRecords = _metaKeyPresetFileService.LoadConfigs();
        var ret = eitherRecords.Map(records =>
        {
            var record = records.GetValueOrDefault("我的收藏");
            if (record == null) return new Result<bool>(new Exception("我的收藏未创建！"));
            IsCombinationKeyFilter = false;
            IsKeyActionFilter = false;

            try
            {
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
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        CombinationKeysConfigs.Add(vm);
                        CombinationKeysConfigsFiltered.Add(vm);
                    });
                }

                foreach (var (k, v) in record.MacroKeys)
                {
                    var vm = new KeyActionConfigForPresetsViewModel
                    {
                        Parent = KeyActionConfigs,
                        Description = k,
                        IsCustomDescription = true
                    };

                    vm.FromKeyActionConfig(v);

                    Dispatcher.UIThread.Invoke(() => KeyActionConfigs.Add(vm));
                }


                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "");
                MessageBox.ShowOverlayAsync(string.Empty, $"读取配置文件失败，具体错误信息为：{e.Message}",
                    hostId: FavPresetsEditorView.LocalHost, icon: MessageBoxIcon.Error);
                // App.GetRequiredService<PopUpNotificationService>()
                //     .Pop(
                //         NotificationType.Error,
                //         $"读取配置文件失败，具体错误信息为：{e.Message}");
                return new Result<bool>(e);
            }
        });
        return ret.Match(s => s, ex => new Result<bool>(ex));
    }

    #endregion

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}
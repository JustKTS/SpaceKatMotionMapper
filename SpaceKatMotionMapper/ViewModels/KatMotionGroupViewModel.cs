using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

/// <summary>
/// 管理一个特定 KatMotion 的所有配置
/// </summary>
public partial class KatMotionGroupViewModel : ObservableObject
{
    private bool _isInternalFirstConfigModeUpdate;
    private bool _isModeChangeConfirming;

#if DEBUG
    public KatMotionGroupViewModel() : this(null!, KatMotionEnum.Null)
    {
    }
#endif

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayName))]
    private KatMotionEnum _katMotion;

    public ObservableCollection<KatMotionViewModel> Configs { get; set; } 
    public KatMotionsWithModeViewModel Parent { get; }

    /// <summary>
    /// 是否为单动作模式（基于第一个配置的模式）
    /// </summary>
    public bool IsSingleActionMode => Configs.FirstOrDefault()?.ConfigMode == KatConfigModeEnum.SingleAction;

    /// <summary>
    /// 是否可以添加更多配置
    /// 单动作模式只能有一个配置，进阶/专家模式可以有多个
    /// </summary>
    public bool CanAddMoreConfigs
    {
        get
        {
            if (Configs.Count == 0) return true;
            return !IsSingleActionMode;
        }
    }

    /// <summary>
    /// 是否是第一个配置（用于启用/禁用 KatMotion 和 ConfigMode 选择）
    /// </summary>
    public bool IsFirstConfig => Configs.Count <= 1;

    /// <summary>
    /// 第一个配置的模式（用于 UI 绑定）
    /// </summary>
    [ObservableProperty]
    public partial KatConfigModeEnum FirstConfigMode { get; set; }

    /// <summary>
    /// 组的可用性状态（所有配置都必须可用）
    /// </summary>
    public bool IsAvailable => Configs.All(c => c.IsAvailable);

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName => KatMotion != KatMotionEnum.Null ? KatMotion.ToStringFast(useMetadataAttributes:true) : "未选择运动方式";

    /// <summary>
    /// 进阶模式下是否隐藏删除按钮（只有一个配置时不允许删除）
    /// </summary>
    public bool HideRemoveButton
    {
        get
        {
            // 单动作模式允许删除（可以删除整个组）
            if (IsSingleActionMode) return false;

            // 进阶/专家模式，只有一个配置时隐藏删除按钮
            return Configs.Count <= 1;
        }
    }

    public KatMotionGroupViewModel(KatMotionsWithModeViewModel parent, KatMotionEnum katMotion)
    {
        Parent = parent;
        Configs = [];

        Configs.CollectionChanged += OnConfigsCollectionChanged;
        
        KatMotion = katMotion;

        // 添加第一个配置
        AddConfig();

        // SAFETY: Check if collection has items before accessing index
        if (Configs.Count > 0)
        {
            FirstConfigMode = Configs[0].ConfigMode;
        }
        else
        {
            // Fallback for empty collection (should not happen under normal circumstances)
            FirstConfigMode = KatConfigModeEnum.SingleAction;
            Log.Warning("[KatMotionGroupViewModel] Constructor: Configs collection is empty after AddConfig()");
        }
    }

    private void OnConfigsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // 处理新增项：订阅 PropertyChanged
        if (e.NewItems != null)
        {
            foreach (KatMotionViewModel item in e.NewItems)
            {
                item.PropertyChanged += ChildPropertyChanged;
            }
        }

        // 处理移除项：取消订阅避免内存泄漏
        if (e.OldItems != null)
        {
            foreach (KatMotionViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        }

        // 通知相关属性变更（Add 和 Remove 都需要）
        OnPropertyChanged(nameof(IsSingleActionMode));
        OnPropertyChanged(nameof(CanAddMoreConfigs));
        OnPropertyChanged(nameof(IsFirstConfig));
        OnPropertyChanged(nameof(FirstConfigMode));
        OnPropertyChanged(nameof(IsAvailable));
        OnPropertyChanged(nameof(HideRemoveButton));
    }

    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // 监听子配置的属性变更
            case nameof(KatMotionViewModel.IsAvailable):
                OnPropertyChanged(nameof(IsAvailable));
                break;
            case nameof(KatMotionViewModel.ConfigMode):
                // 配置模式变更时通知相关属性
                OnPropertyChanged(nameof(IsSingleActionMode));
                OnPropertyChanged(nameof(CanAddMoreConfigs));
                break;
            case nameof(KatMotionViewModel.KatMotion):
                // 如果第一个子配置的 KatMotion 改变，则同步回 Group（双向同步）
                if (sender is KatMotionViewModel child && ReferenceEquals(child, Configs.FirstOrDefault()))
                {
                    if (KatMotion != child.KatMotion)
                    {
                        // 设置 Group 的 KatMotion，会触发 OnKatMotionChanged 并同步到其它子项
                        KatMotion = child.KatMotion;
                    }
                }
                break;
        }
    }

    partial void OnKatMotionChanged(KatMotionEnum oldValue, KatMotionEnum newValue)
    {
        if (oldValue == newValue) return;

        // 将组的 KatMotion 同步到所有子配置，避免不必要的重复赋值
        foreach (var cfg in Configs)
        {
            if (cfg.KatMotion != newValue)
            {
                cfg.KatMotion = newValue;
            }
        }

        // 更新显示名称
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnFirstConfigModeChanging(KatConfigModeEnum oldValue, KatConfigModeEnum newValue)
    {
        if (oldValue == newValue) return;

        // Skip confirmation for internal rollback/apply updates.
        if (_isInternalFirstConfigModeUpdate) return;

        // Prevent overlapping dialogs when binding or user interaction causes rapid changes.
        if (_isModeChangeConfirming) return;

        _isModeChangeConfirming = true;

        Log.Information("[OnFirstConfigModeChanging] Starting mode change: {OldMode} -> {NewMode}, Group KatMotion: {KatMotion}", oldValue, newValue, KatMotion);

        // 保存旧值，以便恢复
        var oldValueToRestore = oldValue;

        _ = ConfirmAndApplyFirstConfigModeChangeAsync(oldValueToRestore, newValue);
    }

    private async System.Threading.Tasks.Task ConfirmAndApplyFirstConfigModeChangeAsync(KatConfigModeEnum oldValueToRestore, KatConfigModeEnum newValue)
    {
        try
        {
            var dialogViewModel = new ConfigModeChangeConfirmDialogViewModel();
            var confirmed = await OverlayDialog.ShowCustomAsync<
                ConfigModeChangeConfirmDialog,
                ConfigModeChangeConfirmDialogViewModel,
                bool>(
                dialogViewModel,
                KatMotionGroupConfigWindow.LocalHost,
                ConfigModeChangeConfirmDialogViewModel.OverlayDialogOptions);

            if (!confirmed)
            {
                Log.Information("[OnFirstConfigModeChanging] User cancelled mode change");
                _isInternalFirstConfigModeUpdate = true;
                try
                {
                    FirstConfigMode = oldValueToRestore;
                }
                finally
                {
                    _isInternalFirstConfigModeUpdate = false;
                }

                return;
            }

            Log.Information("[OnFirstConfigModeChanging] User confirmed mode change, calling RebuildConfigsAfterModeChange");
            RebuildConfigsAfterModeChange(newValue);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[OnFirstConfigModeChanging] Failed while confirming mode change");
        }
        finally
        {
            _isModeChangeConfirming = false;
        }
    }

    private void RebuildConfigsAfterModeChange(KatConfigModeEnum newMode)
    {
        Log.Information("[RebuildConfigsAfterModeChange] Starting rebuild. Group KatMotion: {KatMotion}, Old config count: {OldCount}", KatMotion, Configs.Count);

        try
        {
            // 保存原有配置的数量（用于决定保留多少个配置）
            var oldConfigCount = Configs.Count;

            // 清空现有配置
            Configs.Clear();

            // 如果组的 KatMotion 有效，为每个原有配置创建新配置
            // 如果组的 KatMotion 是 Null，只创建一个空配置
            var configCountToCreate = KatMotion != KatMotionEnum.Null ? oldConfigCount : 1;

            Log.Information("[RebuildConfigsAfterModeChange] Creating {ConfigCount} configs with KatMotion: {KatMotion}", configCountToCreate, KatMotion);

            for (var i = 0; i < configCountToCreate; i++)
            {
                var newConfig = new KatMotionViewModel(this, Parent.ModeNum)
                {
                    KatMotion = KatMotion,  // 使用组的 KatMotion，而不是子配置的
                    ConfigMode = newMode,
                    KatPressMode = KatPressModeEnum.Null,  // 重置为默认
                    RepeatCount = 1  // 重置为默认
                };
                Configs.Add(newConfig);
                Log.Information("[RebuildConfigsAfterModeChange] Created config {Index}: KatMotion={KatMotion}, ConfigMode={ConfigMode}", i, newConfig.KatMotion, newConfig.ConfigMode);
            }

            // 更新 FirstConfigMode 属性
            // 注意：此时 FirstConfigMode 的值还是 oldValue，所以这里会触发属性变更
            FirstConfigMode = newMode;

            // 调试日志
            Log.Information("[RebuildConfigsAfterModeChange] Completed. Group KatMotion: {KatMotion}, Configs Count: {ConfigsCount}, FirstConfigMode: {FirstConfigMode}", KatMotion, Configs.Count, FirstConfigMode);
            Log.Information("[RebuildConfigsAfterModeChange] Each config's KatMotion: {ConfigKatMotions}", string.Join(", ", Configs.Select(c => c.KatMotion.ToString())));

            // 模式切换后重新应用时间配置
            ReApplyTimeConfigs();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[RebuildConfigsAfterModeChange] Failed during rebuild");
            // Ensure at least one config exists
            if (Configs.Count == 0)
            {
                var fallbackConfig = new KatMotionViewModel(this, Parent.ModeNum)
                {
                    KatMotion = KatMotion,
                    ConfigMode = newMode,
                    KatPressMode = KatPressModeEnum.Null,
                    RepeatCount = 1
                };
                Configs.Add(fallbackConfig);
                Log.Warning("[RebuildConfigsAfterModeChange] Created fallback config due to error");
            }
        }
    }

    private void ReApplyTimeConfigs()
    {
        try
        {
            var modeChangeService = App.GetRequiredService<ModeChangeService>();
            var timeConfigService = App.GetRequiredService<KatMotionTimeConfigService>();
            if (modeChangeService.ConfigIsDefault)
                timeConfigService.ApplyDefaultMotionTimeConfig();
            else
                timeConfigService.ApplyMotionTimeConfigById(modeChangeService.CurrentActivatedConfig);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[ReApplyTimeConfigs] Failed to re-apply time configs after mode change");
        }
    }

    /// <summary>
    /// 添加新配置
    /// </summary>
    [RelayCommand]
    private void AddConfig()
    {
        var newConfig = new KatMotionViewModel(this, Parent.ModeNum);

        // 如果不是第一个配置，继承第一个配置的 KatMotion 和 ConfigMode
        // 如果是第一个配置，继承组的 KatMotion
        if (Configs.Count > 0)
        {
            var firstConfig = Configs[0];
            newConfig.KatMotion = firstConfig.KatMotion;
            newConfig.ConfigMode = firstConfig.ConfigMode;
            Log.Debug("[AddConfig] Added config #{Index}. Inherited KatMotion: {KatMotion}, ConfigMode: {ConfigMode}", Configs.Count, newConfig.KatMotion, newConfig.ConfigMode);
        }
        else
        {
            // 第一个配置：继承组的 KatMotion
            newConfig.KatMotion = KatMotion;
            Log.Information("[AddConfig] Added first config. Group KatMotion: {KatMotion}, Config KatMotion: {ConfigKatMotion}", KatMotion, newConfig.KatMotion);
        }

        Configs.Add(newConfig);
        OnPropertyChanged(nameof(IsAvailable));
    }

    /// <summary>
    /// 移除指定索引的配置
    /// </summary>
    [RelayCommand]
    private void RemoveConfig(int index)
    {
        if (index < 0 || index >= Configs.Count) return;

        // 禁止删除最后一个配置（除了单动作模式）
        if (Configs.Count <= 1 && !IsSingleActionMode)
        {
            return;
        }

        Configs.RemoveAt(index);

        // 不再自动添加新配置，允许配置组为空或由用户主动添加
        OnPropertyChanged(nameof(IsAvailable));
    }

    /// <summary>
    /// 移除当前组
    /// </summary>
    [RelayCommand]
    private void RemoveSelf()
    {
        var index = Parent.KatMotionGroups.IndexOf(this);
        if (index >= 0)  // ← Add validation
        {
            Parent.RemoveKatMotionGroupCommand.Execute(index);
        }
        else
        {
            Log.Warning("[RemoveSelf] Cannot find self in parent collection");
        }
    }

    /// <summary>
    /// Cleanup resources when ViewModel is being disposed
    /// </summary>
    public void Cleanup()
    {
        try
        {
            // Unsubscribe from CollectionChanged event
            Configs.CollectionChanged -= OnConfigsCollectionChanged;

            // Unsubscribe from all child PropertyChanged events
            foreach (var config in Configs.ToArray())
            {
                config.PropertyChanged -= ChildPropertyChanged;
            }

            Log.Debug("[KatMotionGroupViewModel] Cleanup completed for KatMotion: {KatMotion}", KatMotion);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[KatMotionGroupViewModel] Error during cleanup");
        }
    }
}

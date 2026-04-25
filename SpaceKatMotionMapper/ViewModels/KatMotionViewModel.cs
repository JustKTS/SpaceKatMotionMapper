using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatMotionViewModel : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private KatMotionEnum _katMotion;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private KatPressModeEnum _katPressMode;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _repeatCount = 1;

    [ObservableProperty] private KeyActionConfigViewModel _keyActionConfigGroup;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _modeNum;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    private int _toModeNum;

    [ObservableProperty] private bool _isAdvancedMode;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsAvailable))]
    [NotifyPropertyChangedFor(nameof(IsSingleActionMode))]
    [NotifyPropertyChangedFor(nameof(IsAdvancedConfigMode))]
    [NotifyPropertyChangedFor(nameof(IsExpertMode))]
    private KatConfigModeEnum _configMode = KatConfigModeEnum.SingleAction;

    public bool IsSingleActionMode => ConfigMode == KatConfigModeEnum.SingleAction;
    public bool IsAdvancedConfigMode => ConfigMode == KatConfigModeEnum.Advanced;
    public bool IsExpertMode => ConfigMode == KatConfigModeEnum.Expert;
    public bool IsAdvancedOrExpertMode => ConfigMode is KatConfigModeEnum.Advanced or KatConfigModeEnum.Expert;

    /// <summary>
    /// 是否可以删除此配置
    /// 单动作模式或进阶/专家模式有多个配置时可以删除
    /// </summary>
    public bool CanRemove => IsSingleActionMode || Parent.Configs.Count > 1;

    // TODO:增加一个Kat长按与释放自动对应按键的长按与释放的检测
    public bool IsAvailable => CheckIsAvailable();

    // TODO：此处的重复性检查，如果两个同时错误，在一个修复后另一个不会恢复正常状态
    private bool CheckIsAvailable()
    {
        // 单动作模式：同一组内只能有一个配置（由 KatMotionGroupViewModel 控制）
        // 进阶/专家模式：同一组内相同KatMotion+PressMode+RepeatCount只能有一个
        var sameCount = 0;
        foreach (var katMotionVm in Parent.Configs)
        {
            bool isDuplicate;
            if (ConfigMode == KatConfigModeEnum.SingleAction)
            {
                // 单动作模式：只检查是否是当前实例本身
                isDuplicate = ReferenceEquals(this, katMotionVm);
            }
            else
            {
                // 进阶/专家模式：检查KatMotion + PressMode + RepeatCount
                isDuplicate = KatMotion == katMotionVm.KatMotion &&
                             KatPressMode == katMotionVm.KatPressMode &&
                             RepeatCount == katMotionVm.RepeatCount &&
                             katMotionVm.ConfigMode != KatConfigModeEnum.SingleAction;
            }

            if (isDuplicate)
            {
                sameCount++;
            }

            if (sameCount > 1) return false;
        }

        // 单动作模式：必须配置有效KatMotion和KeyAction，不需要PressMode
        if (ConfigMode == KatConfigModeEnum.SingleAction)
        {
            return KatMotion is not KatMotionEnum.Null && KeyActionConfigGroup.IsAvailable;
        }

        // 进阶/专家模式：原有检查
        return KatMotion is not KatMotionEnum.Null &&
               KatPressMode is not KatPressModeEnum.Null &&
               KeyActionConfigGroup.IsAvailable;
    }


    public KatMotionGroupViewModel Parent { get; }

    public KatMotionViewModel(KatMotionGroupViewModel parent, int modeNum)
    {
        Parent = parent;
        KatMotion = KatMotionEnum.Null;
        KatPressMode = KatPressModeEnum.Null;
        KeyActionConfigGroup = new KeyActionConfigViewModel(this);
        ModeNum = modeNum;
        ToModeNum = ModeNum;
        KeyActionConfigGroup.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(KeyActionConfigViewModel.IsAvailable)) return;
            OnPropertyChanged(nameof(IsAvailable));
        };
        OnPropertyChanged(nameof(IsAvailable));
    }
    
    public KatMotionConfig ToKatMotionConfig()
    {
        // 单动作模式：强制使用LongReach，RepeatCount=1
        if (ConfigMode == KatConfigModeEnum.SingleAction)
        {
            return new KatMotionConfig(
                new KatMotion(KatMotion, KatPressModeEnum.LongReach, 1),
                KeyActionConfigGroup.ToKeyActionConfigList(),
                KeyActionConfigGroup.IsCustomDescription,
                KeyActionConfigGroup.KeyActionsDescription,
                ModeNum,
                ToModeNum,
                KatConfigModeEnum.SingleAction
            );
        }

        // 进阶/专家模式：使用原有逻辑
        return new KatMotionConfig(
            new KatMotion(KatMotion, KatPressMode, RepeatCount),
            KeyActionConfigGroup.ToKeyActionConfigList(),
            KeyActionConfigGroup.IsCustomDescription,
            KeyActionConfigGroup.KeyActionsDescription,
            ModeNum,
            ToModeNum,
            IsAdvancedMode ? KatConfigModeEnum.Expert : KatConfigModeEnum.Advanced
        );
    }

    /// <summary>
    /// 简单/单动作模式下，生成长推结束的配置（自动将 Press 转换为 Release）
    /// </summary>
    public KatMotionConfig? ToLongDownConfig()
    {
        // 单动作模式：生成长推释放配置
        if (ConfigMode == KatConfigModeEnum.SingleAction)
        {
            var releaseActionConfigs = (
                from actionConfig in KeyActionConfigGroup.ToKeyActionConfigList()
                where actionConfig.PressMode == PressModeEnum.Press
                select actionConfig with { PressMode = PressModeEnum.Release }).ToList();

            return new KatMotionConfig(
                new KatMotion(KatMotion, KatPressModeEnum.LongDown, 1),
                releaseActionConfigs,
                false,
                $"松开: {KeyActionConfigGroup.KeyActionsDescription}",
                ModeNum,
                ToModeNum,
                KatConfigModeEnum.SingleAction
            );
        }

        // 进阶模式（简单模式）：在非专家模式且选择长推时生成
        if (IsAdvancedMode || KatPressMode != KatPressModeEnum.LongReach)
        {
            return null;
        }

        // 将按键配置中的 Press 模式转换为 Release 模式
        var advancedReleaseActionConfigs = (
            from actionConfig in KeyActionConfigGroup.ToKeyActionConfigList()
            where actionConfig.PressMode == PressModeEnum.Press
            select actionConfig with { PressMode = PressModeEnum.Release }).ToList();

        return new KatMotionConfig(
            new KatMotion(KatMotion, KatPressModeEnum.LongDown, RepeatCount),
            advancedReleaseActionConfigs,
            false,
            $"松开: {KeyActionConfigGroup.KeyActionsDescription}",
            ModeNum,
            ToModeNum
        );
    }

    /// <summary>
    /// 单动作模式下，判断当前配置是否属于“单输入动作按住”。
    /// 仅当存在且仅存在一个键盘或鼠标 Press 动作时返回 true。
    /// </summary>
    public bool ShouldEnableAutoRepeatForSingleActionByDefault()
    {
        if (ConfigMode != KatConfigModeEnum.SingleAction)
        {
            return false;
        }

        var actions = KeyActionConfigGroup.ToKeyActionConfigList();
        var pressActions = actions.Where(action =>
            action.ActionType is ActionType.KeyBoard or ActionType.Mouse &&
            action.PressMode == PressModeEnum.Press &&
            action.Key != KeyActionConstants.NoneKeyValue).ToList();

        // 过滤 Delay 后仍有其他动作，按多动作/组合处理（默认仅触发一次）
        var nonDelayActions = actions.Where(action => action.ActionType != ActionType.Delay).ToList();
        return pressActions.Count == 1 && nonDelayActions.Count == 1;
    }

    public bool LoadFromKatMotionConfig(KatMotionConfig motionConfig)
    {
        try
        {
            KatMotion = motionConfig.Motion.Motion;
            KatPressMode = motionConfig.Motion.KatPressMode;
            RepeatCount = motionConfig.Motion.RepeatCount;
            ModeNum = motionConfig.ModeNum;
            ToModeNum = motionConfig.ToModeNum;

            // 加载配置模式并设置IsAdvancedMode
            ConfigMode = motionConfig.ConfigMode;
            IsAdvancedMode = motionConfig.ConfigMode == KatConfigModeEnum.Expert;

            OnPropertyChanged(nameof(IsAvailable));
            if (!motionConfig.IsCustomDescription)
                return KeyActionConfigGroup.FromKeyActionConfig(motionConfig.ActionConfigs);
            KeyActionConfigGroup.IsCustomDescription = true;
            KeyActionConfigGroup.KeyActionsDescription = motionConfig.KeyActionsDescription;

            return KeyActionConfigGroup.FromKeyActionConfig(motionConfig.ActionConfigs);
        }
        catch (Exception e)
        {
            Log.Error(e, "[{ViewModel}] Failed to load KatMotion config", nameof(KatMotionViewModel));
            return false;
        }
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        var index = Parent.Configs.IndexOf(this);
        Parent.RemoveConfigCommand.Execute(index);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Services.Contract;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using Log = Serilog.Log;

namespace SpaceKatMotionMapper.Services;

public class KatMotionActivateService : IKatMotionActivateService
{
    public bool IsActivated { get; set; }

    private event EventHandler<KatDataWithInfo>? KatDataReceived;

    private readonly Dictionary<Guid, EventHandler<KatDataWithInfo>> _katData = [];

    private readonly ModeChangeService _modeChangeService =
        App.GetRequiredService<ModeChangeService>();

    private readonly ConflictKatMotionService _conflictKatMotionService =
        App.GetRequiredService<ConflictKatMotionService>();

    private readonly ActivationStatusService _activationStatusService =
        App.GetRequiredService<ActivationStatusService>();

    private readonly TransparentInfoActionDisplayService _transparentInfoActionDisplayService
        = App.GetRequiredService<TransparentInfoActionDisplayService>();
    
    private readonly IKeyActionExecutor _keyActionExecutor
        = App.GetRequiredService<IKeyActionExecutor>();

    private readonly KatMotionRecognizeService _katMotionRecognizeService;

    private GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();

    private KatMotionWithTimeStamp _lastKatMotionWithTimeStamp = new(KatMotionEnum.Null, KatPressModeEnum.Null, 0);

    public KatMotionActivateService(KatMotionRecognizeService katMotionRecognizeService)
    {
        _katMotionRecognizeService = katMotionRecognizeService;
        _katMotionRecognizeService.DataReceived += (o, data) =>
        {
            if (!IsActivated) return;

            if (!(data.Motion == _lastKatMotionWithTimeStamp.Motion &&
                  data.KatPressMode == _lastKatMotionWithTimeStamp.KatPressMode))
            {
                App.GetRequiredService<TransparentInfoService>().SetActionInfoMotion(false);
            }

            KatDataReceived?.Invoke(o,
                new KatDataWithInfo(_modeChangeService.ConfigIsDefault,
                    _modeChangeService.CurrentActivatedConfig, _modeChangeService.CurrentMode,
                    data.ToKatMotion()));

            _lastKatMotionWithTimeStamp = data;
        };
        GlobalStates.IsMapperEnableChanged += ChangeIsActivated;
    }

    private void ChangeIsActivated(object? sender, bool e)
    {
        IsActivated = e;
    }


    public void ActivateKatMotions(KatMotionConfigGroup configGroup)
    {
        Log.Information("[激活服务] 开始激活配置. 配置组 Guid: {ConfigGuid}, 是否默认配置: {IsDefault}, 进程路径: {ProcessPath}",
            configGroup.Guid, configGroup.IsDefault, configGroup.ProcessPath);

        var id = Guid.Parse(configGroup.Guid);
        Log.Debug("[激活服务] 解析 Guid: {ParsedId}", id);

        // 如果配置已经激活，先停用旧的配置（防止重复键错误）
        if (_katData.ContainsKey(id))
        {
            Log.Warning("[激活服务] 配置已存在，先停用旧配置. Guid: {Guid}", id);
            DeactivateKatMotions(configGroup);
        }

        Log.Debug("[激活服务] 开始组装事件处理器. 配置数量: {ConfigCount}", configGroup.Motions.Count);
        var handler = AssembleKatEvent(configGroup);

        _katData.Add(id, handler);
        KatDataReceived += handler;

        Log.Information("[激活服务] 事件处理器已注册，添加到 _katData 字典. Guid: {Guid}", id);
        _modeChangeService.UpdateBindProcessPathList(configGroup);
        _activationStatusService.SetActivationStatus(id, true);

        Log.Information("[激活服务] 配置激活完成. Guid: {Guid}", id);
    }

    public void DeactivateKatMotions(KatMotionConfigGroup configGroup)
    {
        var id = Guid.Parse(configGroup.Guid);
        if (!_katData.TryGetValue(id, out var handler)) return;

        KatDataReceived -= handler;
        _katData.Remove(id);
        if (!configGroup.IsDefault)
        {
            _conflictKatMotionService.RemoveByGuid(id);
        }

        _modeChangeService.RemovePathForBindProcessPathList(configGroup.ProcessPath);
        _activationStatusService.SetActivationStatus(id, false);
    }

    private EventHandler<KatDataWithInfo> AssembleKatEvent(KatMotionConfigGroup configGroup)
    {
        List<Action<KatDataWithInfo>> actions = [];
        if (!configGroup.IsDefault)
        {
            configGroup.Motions.Iter(e =>
            {
                var info = new KatMotionInfo(
                    Guid.Parse(configGroup.Guid),
                    new KatMotion(e.Motion.Motion, e.Motion.KatPressMode, e.Motion.RepeatCount));
                _conflictKatMotionService.Register(info);
            });
        }

        var motionId = Guid.Parse(configGroup.Guid);

        _transparentInfoActionDisplayService.ClearMotionGroup(motionId);
        Dictionary<Guid, KatMotionConfig> motionWithGuids = [];
        configGroup.Motions.Iter(config =>
        {
            var displayId = Guid.CreateVersion7();
            motionWithGuids[displayId] = config;
            _transparentInfoActionDisplayService.Register(motionId, displayId, config.GetKeyActionsDescriptions());
        });

        actions.AddRange(motionWithGuids.Select(keyValue =>
            (Action<KatDataWithInfo>)(dataWithInfo =>
            {
                var (displayId, config) = keyValue;
                if (dataWithInfo.ConfigIsDefault && !configGroup.IsDefault) return;
                if (!configGroup.IsDefault && motionId != dataWithInfo.ActivatedConfigId) return;
                if (!dataWithInfo.KatMotion.MatchesMotion(config.Motion)) return;

                if (dataWithInfo.Mode != config.ModeNum) return;

                if (configGroup.IsDefault && !dataWithInfo.ConfigIsDefault &&
                    _conflictKatMotionService.IsConflict(dataWithInfo.ActivatedConfigId, config.Motion.Motion,
                        config.Motion.KatPressMode,
                        config.Motion.RepeatCount)) return;
                    
                App.GetRequiredService<TransparentInfoService>()
                    .SetActionInfoMotion(true, _transparentInfoActionDisplayService.GetDisplay(motionId, displayId));

                _keyActionExecutor.ExecuteActions(config.ActionConfigs);

                if (_modeChangeService.CurrentMode != config.ToModeNum)
                {
                    _modeChangeService.CurrentMode = config.ToModeNum;
                }
            })));
        return (_, data) => { actions.Iter(action => action.Invoke(data)); };
    }
}
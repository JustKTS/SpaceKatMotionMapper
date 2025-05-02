using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.States;
using WindowsInput;

namespace SpaceKatMotionMapper.Services;

public class KatMotionActivateService
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

    private readonly KatMotionRecognizeService _katMotionRecognizeService;
    private readonly InputSimulator _inputSimulator;

    private GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();

    private KatMotionWithTimeStamp _lastKatMotionWithTimeStamp =
        new KatMotionWithTimeStamp(KatMotionEnum.Null, KatPressModeEnum.Null, 0);

    public KatMotionActivateService(InputSimulator inputSimulator,
        KatMotionRecognizeService katMotionRecognizeService)
    {
        _inputSimulator = inputSimulator;
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
        var id = Guid.Parse(configGroup.Guid);
        var handler = AssembleKatEvent(configGroup);
        _katData.Add(id, handler);
        KatDataReceived += handler;
        _modeChangeService.UpdateBindProcessPathList(configGroup);
        _activationStatusService.SetActivationStatus(id, true);
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
                if (dataWithInfo.KatMotion.Motion != config.Motion.Motion) return;
                if (dataWithInfo.KatMotion.KatPressMode != config.Motion.KatPressMode) return;
                if (dataWithInfo.KatMotion.KatPressMode != KatPressModeEnum.LongReach)
                {
                    if (dataWithInfo.KatMotion.RepeatCount != config.Motion.RepeatCount) return;
                }

                if (dataWithInfo.Mode != config.ModeNum) return;

                if (configGroup.IsDefault && !dataWithInfo.ConfigIsDefault &&
                    _conflictKatMotionService.IsConflict(dataWithInfo.ActivatedConfigId, config.Motion.Motion,
                        config.Motion.KatPressMode,
                        config.Motion.RepeatCount)) return;

                App.GetRequiredService<TransparentInfoService>()
                    .SetActionInfoMotion(true, _transparentInfoActionDisplayService.GetDisplay(motionId, displayId));

                KeyActionExecutor.ExecuteActions(_inputSimulator, config.ActionConfigs);

                if (_modeChangeService.CurrentMode != config.ToModeNum)
                {
                    _modeChangeService.CurrentMode = config.ToModeNum;
                }
            })));
        return (_, data) => { actions.Iter(action => action.Invoke(data)); };
    }
}
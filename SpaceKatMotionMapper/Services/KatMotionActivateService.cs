using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceKatHIDWrapper.Models;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.States;
using WindowsInput;

namespace SpaceKatMotionMapper.Services;

public class KatMotionActivateService
{
    public bool IsActivated { get; set; }

    private EventHandler<KatDataWithInfo>? _katDataReceived;

    private readonly Dictionary<Guid, EventHandler<KatDataWithInfo>> _katData = [];

    private readonly ModeChangeService _modeChangeService =
        App.GetRequiredService<ModeChangeService>();

    private readonly ConflictKatMotionService _conflictKatMotionService =
        App.GetRequiredService<ConflictKatMotionService>();

    private readonly ActivationStatusService _activationStatusService =
        App.GetRequiredService<ActivationStatusService>();

    private readonly KatMotionRecognizeService _katMotionRecognizeService;
    private readonly InputSimulator _inputSimulator;
    
    private GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();

    public KatMotionActivateService(InputSimulator inputSimulator,
        KatMotionRecognizeService katMotionRecognizeService)
    {
        _inputSimulator = inputSimulator;
        _katMotionRecognizeService = katMotionRecognizeService;
        _katMotionRecognizeService.DataReceived += (o, data) =>
        {
            if (!IsActivated) return;
            _katDataReceived?.Invoke(o,
                new KatDataWithInfo(_modeChangeService.ConfigIsDefault,
                    _modeChangeService.CurrentActivatedConfig, _modeChangeService.CurrentMode,
                    data.ToKatMotion()));
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
        _katDataReceived += handler;
        _modeChangeService.UpdateBindProcessPathList(configGroup);
        _activationStatusService.SetActivationStatus(id, true);
    }

    public void DeactivateKatMotions(KatMotionConfigGroup configGroup)
    {
        var id = Guid.Parse(configGroup.Guid);
        if (!_katData.TryGetValue(id, out var handler)) return;

        _katDataReceived -= handler;
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

        actions.AddRange(configGroup.Motions.Select(config =>
            (Action<KatDataWithInfo>)(dataWithInfo =>
            {
                var id = Guid.Parse(configGroup.Guid);
                if (dataWithInfo.ConfigIsDefault && !configGroup.IsDefault) return;
                if (!configGroup.IsDefault && id != dataWithInfo.ActivatedConfigId) return;
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

                foreach (var actionConfig in config.ActionConfigs)
                {
                    if (actionConfig.TryToMouseActionConfig(out var mouseActionConfig))
                    {
                        MouseActionHandler(mouseActionConfig);
                    }

                    if (actionConfig.TryToKeyBoardActionConfig(out var keyboardActionConfig))
                    {
                        KeyBoardActionHandler(keyboardActionConfig);
                    }
                    if (actionConfig.TryToDelayActionConfig(out var delayActionConfig))
                    {
                        Thread.Sleep(delayActionConfig.Milliseconds);
                    }
                }

                if (_modeChangeService.CurrentMode != config.ToModeNum)
                {
                    _modeChangeService.CurrentMode = config.ToModeNum;
                }
            })));
        return (_, data) =>
        {
            foreach (var action in actions)
            {
                action.Invoke(data);
            }
        };
    }

    private void MouseActionHandler(MouseActionConfig mouseActionConfig)
    {
        switch (mouseActionConfig.Key)
        {
            case MouseButtonEnum.None:
                break;
            case MouseButtonEnum.LButton:
                switch (mouseActionConfig.PressMode)
                {
                    case PressModeEnum.Click:
                        _inputSimulator.Mouse.LeftButtonClick();
                        break;
                    case PressModeEnum.Release:
                        _inputSimulator.Mouse.LeftButtonUp();
                        break;
                    case PressModeEnum.Press:
                        _inputSimulator.Mouse.LeftButtonDown();
                        break;
                    case PressModeEnum.DoubleClick:
                        _inputSimulator.Mouse.RightButtonDoubleClick();
                        break;
                    case PressModeEnum.None:
                    default:
                        break;
                }

                break;
            case MouseButtonEnum.RButton:
                switch (mouseActionConfig.PressMode)
                {
                    case PressModeEnum.Click:
                        _inputSimulator.Mouse.RightButtonClick();
                        break;
                    case PressModeEnum.Release:
                        _inputSimulator.Mouse.RightButtonUp();
                        break;
                    case PressModeEnum.Press:
                        _inputSimulator.Mouse.RightButtonDown();
                        break;
                    case PressModeEnum.DoubleClick:
                        _inputSimulator.Mouse.RightButtonDoubleClick();
                        break;
                    case PressModeEnum.None:
                    default:
                        break;
                }

                break;
            case MouseButtonEnum.MButton:
                switch (mouseActionConfig.PressMode)
                {
                    case PressModeEnum.Click:
                        _inputSimulator.Mouse.MiddleButtonClick();
                        break;
                    case PressModeEnum.Release:
                        _inputSimulator.Mouse.MiddleButtonUp();
                        break;
                    case PressModeEnum.Press:
                        _inputSimulator.Mouse.MiddleButtonDown();
                        break;
                    case PressModeEnum.DoubleClick:
                        _inputSimulator.Mouse.MiddleButtonDoubleClick();
                        break;
                    case PressModeEnum.None:
                    default:
                        break;
                }

                break;
            case MouseButtonEnum.ScrollUp:
                _inputSimulator.Mouse.VerticalScroll(mouseActionConfig.Multiplier);
                break;
            case MouseButtonEnum.ScrollDown:
                _inputSimulator.Mouse.VerticalScroll(-1 * mouseActionConfig.Multiplier);
                break;
            default:
                throw new Exception("No mouse action configured");
        }
    }

    private void KeyBoardActionHandler(KeyBoardActionConfig keyBoardActionConfig)
    {
        switch (keyBoardActionConfig.PressMode)
        {
            case PressModeEnum.Click:
                _inputSimulator.Keyboard.KeyPress(keyBoardActionConfig.Key);
                break;
            case PressModeEnum.Release:
                _inputSimulator.Keyboard.KeyUp(keyBoardActionConfig.Key);
                break;
            case PressModeEnum.Press:
                _inputSimulator.Keyboard.KeyDown(keyBoardActionConfig.Key);
                break;
            case PressModeEnum.None:
            case PressModeEnum.DoubleClick:
            default:
                break;
        }
    }
}
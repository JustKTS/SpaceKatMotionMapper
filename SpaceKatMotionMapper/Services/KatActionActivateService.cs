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

public class KatActionActivateService
{
    public bool IsActivated { get; set; }

    private EventHandler<KatDataWithInfo>? _katDataReceived;

    private readonly Dictionary<Guid, EventHandler<KatDataWithInfo>> _katData = [];

    private readonly ModeChangeService _modeChangeService =
        App.GetRequiredService<ModeChangeService>();

    private readonly ConflictKatActionService _conflictKatActionService =
        App.GetRequiredService<ConflictKatActionService>();

    private readonly ActivationStatusService _activationStatusService =
        App.GetRequiredService<ActivationStatusService>();

    private readonly KatActionRecognizeService _katActionRecognizeService;
    private readonly InputSimulator _inputSimulator;
    
    private GlobalStates GlobalStates => App.GetRequiredService<GlobalStates>();

    public KatActionActivateService(InputSimulator inputSimulator,
        KatActionRecognizeService katActionRecognizeService)
    {
        _inputSimulator = inputSimulator;
        _katActionRecognizeService = katActionRecognizeService;
        _katActionRecognizeService.DataReceived += (o, data) =>
        {
            if (!IsActivated) return;
            _katDataReceived?.Invoke(o,
                new KatDataWithInfo(_modeChangeService.ConfigIsDefault,
                    _modeChangeService.CurrentActivatedConfig, _modeChangeService.CurrentMode,
                    data));
        };
        GlobalStates.IsMapperEnableChanged += ChangeIsActivated;
    }

    private void ChangeIsActivated(object? sender, bool e)
    {
        IsActivated = e;
    }


    public void ActivateKatActions(KatActionConfigGroup configGroup)
    {
        var id = Guid.Parse(configGroup.Guid);
        var handler = AssembleKatEvent(configGroup);
        _katData.Add(id, handler);
        _katDataReceived += handler;
        _modeChangeService.UpdateBindProcessPathList(configGroup);
        _activationStatusService.SetActivationStatus(id, true);
    }

    public void DeactivateKatActions(KatActionConfigGroup configGroup)
    {
        var id = Guid.Parse(configGroup.Guid);
        if (!_katData.TryGetValue(id, out var handler)) return;

        _katDataReceived -= handler;
        _katData.Remove(id);
        if (!configGroup.IsDefault)
        {
            _conflictKatActionService.RemoveByGuid(id);
        }

        _modeChangeService.RemovePathForBindProcessPathList(configGroup.ProcessPath);
        _activationStatusService.SetActivationStatus(id, false);
    }

    private EventHandler<KatDataWithInfo> AssembleKatEvent(KatActionConfigGroup configGroup)
    {
        List<Action<KatDataWithInfo>> actions = [];
        if (!configGroup.IsDefault)
        {
            configGroup.Actions.Iter(e =>
            {
                var info = new KatActionInfo(
                    Guid.Parse(configGroup.Guid),
                    new KatAction(e.Action.Motion, e.Action.KatPressMode, e.Action.RepeatCount));
                _conflictKatActionService.Register(info);
            });
        }

        actions.AddRange(configGroup.Actions.Select(config =>
            (Action<KatDataWithInfo>)(dataWithInfo =>
            {
                var id = Guid.Parse(configGroup.Guid);
                if (dataWithInfo.ConfigIsDefault && !configGroup.IsDefault) return;
                if (!configGroup.IsDefault && id != dataWithInfo.ActivatedConfigId) return;
                if (dataWithInfo.KatAction.Motion != config.Action.Motion) return;
                if (dataWithInfo.KatAction.KatPressMode != config.Action.KatPressMode) return;
                if (dataWithInfo.KatAction.KatPressMode != KatPressModeEnum.LongReach)
                {
                    if (dataWithInfo.KatAction.RepeatCount != config.Action.RepeatCount) return;
                }

                if (dataWithInfo.Mode != config.ModeNum) return;

                if (configGroup.IsDefault && !dataWithInfo.ConfigIsDefault &&
                    _conflictKatActionService.IsConflict(dataWithInfo.ActivatedConfigId, config.Action.Motion,
                        config.Action.KatPressMode,
                        config.Action.RepeatCount)) return;

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
                    // TODO:验证延时的可靠性
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
#if WINDOWS
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using WindowsInput;

namespace Win32Helpers.Services.Input;

public class KeyActionExecutorWindows(IInputSimulator inputSimulator) : IKeyActionExecutor
{
    public void MouseActionHandler(MouseActionConfig mouseActionConfig)
    {
        switch (mouseActionConfig.Key)
        {
            case MouseButtonEnum.None:
                break;
            case MouseButtonEnum.LButton:
                switch (mouseActionConfig.PressMode)
                {
                    case PressModeEnum.Click:
                        inputSimulator.Mouse.LeftButtonClick();
                        break;
                    case PressModeEnum.Release:
                        inputSimulator.Mouse.LeftButtonUp();
                        break;
                    case PressModeEnum.Press:
                        inputSimulator.Mouse.LeftButtonDown();
                        break;
                    case PressModeEnum.DoubleClick:
                        inputSimulator.Mouse.RightButtonDoubleClick();
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
                        inputSimulator.Mouse.RightButtonClick();
                        break;
                    case PressModeEnum.Release:
                        inputSimulator.Mouse.RightButtonUp();
                        break;
                    case PressModeEnum.Press:
                        inputSimulator.Mouse.RightButtonDown();
                        break;
                    case PressModeEnum.DoubleClick:
                        inputSimulator.Mouse.RightButtonDoubleClick();
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
                        inputSimulator.Mouse.MiddleButtonClick();
                        break;
                    case PressModeEnum.Release:
                        inputSimulator.Mouse.MiddleButtonUp();
                        break;
                    case PressModeEnum.Press:
                        inputSimulator.Mouse.MiddleButtonDown();
                        break;
                    case PressModeEnum.DoubleClick:
                        inputSimulator.Mouse.MiddleButtonDoubleClick();
                        break;
                    case PressModeEnum.None:
                    default:
                        break;
                }

                break;
            case MouseButtonEnum.ScrollUp:
                inputSimulator.Mouse.VerticalScroll(mouseActionConfig.Multiplier);
                break;
            case MouseButtonEnum.ScrollDown:
                inputSimulator.Mouse.VerticalScroll(-1 * mouseActionConfig.Multiplier);
                break;
            case MouseButtonEnum.ScrollLeft:
                inputSimulator.Mouse.HorizontalScroll(-1 * mouseActionConfig.Multiplier);
                break;
            case MouseButtonEnum.ScrollRight:
                inputSimulator.Mouse.HorizontalScroll(mouseActionConfig.Multiplier);
                break;
            default:
                throw new Exception("No mouse action configured");
        }
    }

    public void KeyBoardActionHandler(KeyBoardActionConfig keyBoardActionConfig)
    {
        var virtualKey = (VirtualKeyCode)(int)keyBoardActionConfig.Key;
        switch (keyBoardActionConfig.PressMode)
        {
            case PressModeEnum.Click:
                inputSimulator.Keyboard.KeyPress(virtualKey);
                break;
            case PressModeEnum.Release:
                inputSimulator.Keyboard.KeyUp(virtualKey);
                break;
            case PressModeEnum.Press:
                inputSimulator.Keyboard.KeyDown(virtualKey);
                break;
            case PressModeEnum.None:
            case PressModeEnum.DoubleClick:
            default:
                break;
        }
    }

    public void ExecuteActions(IEnumerable<KeyActionConfig> configs)
    {
        foreach (var actionConfig in configs)
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
    }
}
#endif

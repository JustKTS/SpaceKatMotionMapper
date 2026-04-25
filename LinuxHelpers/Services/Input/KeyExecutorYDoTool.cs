using System.Diagnostics;
using SpaceKat.Shared.Models;
// ReSharper disable CommentTypo

namespace LinuxHelpers.Services.Input;

public static class KeyExecutorYDoTool
{
    private static bool IsYDoToolAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "ydotool",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            _ = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // 如果找到命令，which会输出路径；否则输出为空
            return !string.IsNullOrEmpty(output.Trim());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"检查wtype时出错: {ex.Message}");
            return false;
        }
    }

    public static bool ExecuteMouseAction(MouseActionConfig mouseActionConfig)
    {
        if (!IsYDoToolAvailable())
        {
            return false;
        }

        switch (mouseActionConfig.Key)
        {
            case MouseButtonEnum.None:
                break;
            case MouseButtonEnum.LButton:
                switch (mouseActionConfig.PressMode)
                {
                    case PressModeEnum.Click:
                        ExecuteYDoToolCommand("click 0xC0"); // 左键单击
                        break;
                    case PressModeEnum.Release:
                        ExecuteYDoToolCommand("mouseup 0xC0"); // 左键释放
                        break;
                    case PressModeEnum.Press:
                        ExecuteYDoToolCommand("mousedown 0xC0"); // 左键按下
                        break;
                    case PressModeEnum.DoubleClick:
                        // 双击可以模拟为两次单击
                        ExecuteYDoToolCommand("click 0xC0");
                        Thread.Sleep(50); // 添加短暂延迟
                        ExecuteYDoToolCommand("click 0xC0");
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
                        ExecuteYDoToolCommand("click 0xC1"); // 右键单击
                        break;
                    case PressModeEnum.Release:
                        ExecuteYDoToolCommand("mouseup 0xC1"); // 右键释放
                        break;
                    case PressModeEnum.Press:
                        ExecuteYDoToolCommand("mousedown 0xC1"); // 右键按下
                        break;
                    case PressModeEnum.DoubleClick:
                        ExecuteYDoToolCommand("click 0xC1");
                        Thread.Sleep(50);
                        ExecuteYDoToolCommand("click 0xC1");
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
                        ExecuteYDoToolCommand("click 0xC2"); // 中键单击
                        break;
                    case PressModeEnum.Release:
                        ExecuteYDoToolCommand("mouseup 0xC2"); // 中键释放
                        break;
                    case PressModeEnum.Press:
                        ExecuteYDoToolCommand("mousedown 0xC2"); // 中键按下
                        break;
                    case PressModeEnum.DoubleClick:
                        ExecuteYDoToolCommand("click 0xC2");
                        Thread.Sleep(50);
                        ExecuteYDoToolCommand("click 0xC2");
                        break;
                    case PressModeEnum.None:
                    default:
                        break;
                }

                break;

            case MouseButtonEnum.ScrollUp:
                ExecuteYDoToolCommand($"mousemove -w -- 0 {mouseActionConfig.Multiplier}");
                break;
            case MouseButtonEnum.ScrollDown:
                ExecuteYDoToolCommand($"mousemove -w -- 0 -{mouseActionConfig.Multiplier}");
                break;
            case MouseButtonEnum.ScrollLeft:
                ExecuteYDoToolCommand($"mousemove -w -- -{mouseActionConfig.Multiplier} 0");
                break;
            case MouseButtonEnum.ScrollRight:
                ExecuteYDoToolCommand($"mousemove -w -- {mouseActionConfig.Multiplier} 0");
                break;
            default:
                throw new Exception("No mouse action configured");
        }

        return true;
    }

    public static void KeyBoardActionHandler(KeyBoardActionConfig keyBoardActionConfig)
{
    if (!IsYDoToolAvailable())
    {
        return;
    }

    var ydotoolKeyCode = GetYdotoolKeyCode(keyBoardActionConfig.Key);

    if (string.IsNullOrEmpty(ydotoolKeyCode))
    {
        Console.WriteLine($"未找到键码映射: {keyBoardActionConfig.Key}");
        return;
    }

    switch (keyBoardActionConfig.PressMode)
    {
        case PressModeEnum.Click:
            ExecuteYDoToolCommand($"key {ydotoolKeyCode}:1 {ydotoolKeyCode}:0");
            break;
        case PressModeEnum.Release:
            ExecuteYDoToolCommand($"key {ydotoolKeyCode}:0");
            break;
        case PressModeEnum.Press:
            ExecuteYDoToolCommand($"key {ydotoolKeyCode}:1");
            break;
        case PressModeEnum.None:
        case PressModeEnum.DoubleClick:
        default:
            break;
    }
}
private static string? GetYdotoolKeyCode(KeyCodeWrapper key)
{
    // 将 KeyCodeWrapper 映射到 Linux 键码
    // Linux 键码定义在 /usr/include/linux/input-event-codes.h
    switch (key)
    {
        // 字母键
        case KeyCodeWrapper.A: return "30";    // KEY_A
        case KeyCodeWrapper.B: return "48";    // KEY_B
        case KeyCodeWrapper.C: return "46";    // KEY_C
        case KeyCodeWrapper.D: return "32";    // KEY_D
        case KeyCodeWrapper.E: return "18";    // KEY_E
        case KeyCodeWrapper.F: return "33";    // KEY_F
        case KeyCodeWrapper.G: return "34";    // KEY_G
        case KeyCodeWrapper.H: return "35";    // KEY_H
        case KeyCodeWrapper.I: return "23";    // KEY_I
        case KeyCodeWrapper.J: return "36";    // KEY_J
        case KeyCodeWrapper.K: return "37";    // KEY_K
        case KeyCodeWrapper.L: return "38";    // KEY_L
        case KeyCodeWrapper.M: return "50";    // KEY_M
        case KeyCodeWrapper.N: return "49";    // KEY_N
        case KeyCodeWrapper.O: return "24";    // KEY_O
        case KeyCodeWrapper.P: return "25";    // KEY_P
        case KeyCodeWrapper.Q: return "16";    // KEY_Q
        case KeyCodeWrapper.R: return "19";    // KEY_R
        case KeyCodeWrapper.S: return "31";    // KEY_S
        case KeyCodeWrapper.T: return "20";    // KEY_T
        case KeyCodeWrapper.U: return "22";    // KEY_U
        case KeyCodeWrapper.V: return "47";    // KEY_V
        case KeyCodeWrapper.W: return "17";    // KEY_W
        case KeyCodeWrapper.X: return "45";    // KEY_X
        case KeyCodeWrapper.Y: return "21";    // KEY_Y
        case KeyCodeWrapper.Z: return "44";    // KEY_Z

        // 数字键
        case KeyCodeWrapper.NUM_0: return "11";    // KEY_0
        case KeyCodeWrapper.NUM_1: return "2";     // KEY_1
        case KeyCodeWrapper.NUM_2: return "3";     // KEY_2
        case KeyCodeWrapper.NUM_3: return "4";     // KEY_3
        case KeyCodeWrapper.NUM_4: return "5";     // KEY_4
        case KeyCodeWrapper.NUM_5: return "6";     // KEY_5
        case KeyCodeWrapper.NUM_6: return "7";     // KEY_6
        case KeyCodeWrapper.NUM_7: return "8";     // KEY_7
        case KeyCodeWrapper.NUM_8: return "9";     // KEY_8
        case KeyCodeWrapper.NUM_9: return "10";    // KEY_9

        // 功能键
        case KeyCodeWrapper.F1: return "59";      // KEY_F1
        case KeyCodeWrapper.F2: return "60";      // KEY_F2
        case KeyCodeWrapper.F3: return "61";      // KEY_F3
        case KeyCodeWrapper.F4: return "62";      // KEY_F4
        case KeyCodeWrapper.F5: return "63";      // KEY_F5
        case KeyCodeWrapper.F6: return "64";      // KEY_F6
        case KeyCodeWrapper.F7: return "65";      // KEY_F7
        case KeyCodeWrapper.F8: return "66";      // KEY_F8
        case KeyCodeWrapper.F9: return "67";      // KEY_F9
        case KeyCodeWrapper.F10: return "68";     // KEY_F10
        case KeyCodeWrapper.F11: return "87";     // KEY_F11
        case KeyCodeWrapper.F12: return "88";     // KEY_F12
        case KeyCodeWrapper.F13: return "183";    // KEY_F13
        case KeyCodeWrapper.F14: return "184";    // KEY_F14
        case KeyCodeWrapper.F15: return "185";    // KEY_F15
        case KeyCodeWrapper.F16: return "186";    // KEY_F16
        case KeyCodeWrapper.F17: return "187";    // KEY_F17
        case KeyCodeWrapper.F18: return "188";    // KEY_F18
        case KeyCodeWrapper.F19: return "189";    // KEY_F19
        case KeyCodeWrapper.F20: return "190";    // KEY_F20
        case KeyCodeWrapper.F21: return "191";    // KEY_F21
        case KeyCodeWrapper.F22: return "192";    // KEY_F22
        case KeyCodeWrapper.F23: return "193";    // KEY_F23
        case KeyCodeWrapper.F24: return "194";    // KEY_F24

        // 控制键
        case KeyCodeWrapper.CONTROL: return "29";    // KEY_LEFTCTRL
        case KeyCodeWrapper.LCONTROL: return "29";   // KEY_LEFTCTRL
        case KeyCodeWrapper.RCONTROL: return "97";   // KEY_RIGHTCTRL
        case KeyCodeWrapper.SHIFT: return "42";      // KEY_LEFTSHIFT
        case KeyCodeWrapper.LSHIFT: return "42";     // KEY_LEFTSHIFT
        case KeyCodeWrapper.RSHIFT: return "54";     // KEY_RIGHTSHIFT
        case KeyCodeWrapper.ALT: return "56";        // KEY_LEFTALT
        case KeyCodeWrapper.LALT: return "56";       // KEY_LEFTALT
        case KeyCodeWrapper.RALT: return "100";      // KEY_RIGHTALT
        case KeyCodeWrapper.LWIN: return "125";      // KEY_LEFTMETA (Windows键)
        case KeyCodeWrapper.RWIN: return "126";      // KEY_RIGHTMETA
        case KeyCodeWrapper.APPS: return "127";      // KEY_COMPOSE (菜单键)

        // 导航键
        case KeyCodeWrapper.TAB: return "15";       // KEY_TAB
        case KeyCodeWrapper.CAPITAL: return "58";   // KEY_CAPSLOCK
        case KeyCodeWrapper.RETURN: return "28";    // KEY_ENTER
        case KeyCodeWrapper.ESCAPE: return "1";     // KEY_ESC
        case KeyCodeWrapper.SPACE: return "57";     // KEY_SPACE
        case KeyCodeWrapper.BACK: return "14";      // KEY_BACKSPACE
        case KeyCodeWrapper.DELETE: return "111";   // KEY_DELETE
        case KeyCodeWrapper.INSERT: return "110";   // KEY_INSERT
        case KeyCodeWrapper.HOME: return "102";     // KEY_HOME
        case KeyCodeWrapper.END: return "107";      // KEY_END
        case KeyCodeWrapper.PRIOR: return "104";   // KEY_PAGEUP
        case KeyCodeWrapper.NEXT: return "109"; // KEY_PAGEDOWN

        // 方向键
        case KeyCodeWrapper.UP: return "103";      // KEY_UP
        case KeyCodeWrapper.DOWN: return "108";    // KEY_DOWN
        case KeyCodeWrapper.LEFT: return "105";    // KEY_LEFT
        case KeyCodeWrapper.RIGHT: return "106";   // KEY_RIGHT

        // 小键盘
        case KeyCodeWrapper.NUMPAD0: return "82";    // KEY_KP0
        case KeyCodeWrapper.NUMPAD1: return "79";    // KEY_KP1
        case KeyCodeWrapper.NUMPAD2: return "80";    // KEY_KP2
        case KeyCodeWrapper.NUMPAD3: return "81";    // KEY_KP3
        case KeyCodeWrapper.NUMPAD4: return "75";    // KEY_KP4
        case KeyCodeWrapper.NUMPAD5: return "76";    // KEY_KP5
        case KeyCodeWrapper.NUMPAD6: return "77";    // KEY_KP6
        case KeyCodeWrapper.NUMPAD7: return "71";    // KEY_KP7
        case KeyCodeWrapper.NUMPAD8: return "72";    // KEY_KP8
        case KeyCodeWrapper.NUMPAD9: return "73";    // KEY_KP9
        case KeyCodeWrapper.MULTIPLY: return "55";   // KEY_KPASTERISK
        case KeyCodeWrapper.ADD: return "78";        // KEY_KPPLUS
        case KeyCodeWrapper.SUBTRACT: return "74";   // KEY_KPMINUS
        case KeyCodeWrapper.DECIMAL: return "83";    // KEY_KPDOT
        case KeyCodeWrapper.DIVIDE: return "98";     // KEY_KPSLASH
        case KeyCodeWrapper.NUMLOCK: return "69";    // KEY_NUMLOCK
        case KeyCodeWrapper.SCROLL: return "70";     // KEY_SCROLLLOCK

        // 符号键
        case KeyCodeWrapper.OEM_1: return "39";      // KEY_SEMICOLON
        case KeyCodeWrapper.OEM_PLUS: return "13";   // KEY_EQUAL
        case KeyCodeWrapper.OEM_COMMA: return "51";  // KEY_COMMA
        case KeyCodeWrapper.OEM_MINUS: return "12";  // KEY_MINUS
        case KeyCodeWrapper.OEM_PERIOD: return "52"; // KEY_DOT
        case KeyCodeWrapper.OEM_2: return "53";      // KEY_SLASH
        case KeyCodeWrapper.OEM_3: return "41";      // KEY_GRAVE
        case KeyCodeWrapper.OEM_4: return "26";      // KEY_LEFTBRACE
        case KeyCodeWrapper.OEM_5: return "43";      // KEY_BACKSLASH
        case KeyCodeWrapper.OEM_6: return "27";      // KEY_RIGHTBRACE
        case KeyCodeWrapper.OEM_7: return "40";      // KEY_APOSTROPHE
        case KeyCodeWrapper.OEM_102: return "86";    // KEY_102ND

        // 其他功能键
        case KeyCodeWrapper.PAUSE: return "119";     // KEY_PAUSE
        case KeyCodeWrapper.PRINT: return "99";      // KEY_SYSRQ (Print Screen)
        case KeyCodeWrapper.SNAPSHOT: return "99";   // KEY_SYSRQ
        case KeyCodeWrapper.HELP: return "138";      // KEY_HELP

        // 媒体键
        case KeyCodeWrapper.VOLUME_MUTE: return "113";    // KEY_MUTE
        case KeyCodeWrapper.VOLUME_DOWN: return "114";    // KEY_VOLUMEDOWN
        case KeyCodeWrapper.VOLUME_UP: return "115";      // KEY_VOLUMEUP
        case KeyCodeWrapper.MEDIA_NEXT_TRACK: return "163"; // KEY_NEXTSONG
        case KeyCodeWrapper.MEDIA_PREV_TRACK: return "165"; // KEY_PREVIOUSSONG
        case KeyCodeWrapper.MEDIA_STOP: return "166";       // KEY_STOPCD
        case KeyCodeWrapper.MEDIA_PLAY_PAUSE: return "164"; // KEY_PLAYPAUSE

        // 浏览器键
        case KeyCodeWrapper.BROWSER_BACK: return "158";    // KEY_BACK
        case KeyCodeWrapper.BROWSER_FORWARD: return "159"; // KEY_FORWARD
        case KeyCodeWrapper.BROWSER_REFRESH: return "173"; // KEY_REFRESH
        case KeyCodeWrapper.BROWSER_STOP: return "174";    // KEY_STOP
        case KeyCodeWrapper.BROWSER_SEARCH: return "217";  // KEY_SEARCH
        case KeyCodeWrapper.BROWSER_FAVORITES: return "156"; // KEY_FAVORITES
        case KeyCodeWrapper.BROWSER_HOME: return "172";    // KEY_HOMEPAGE

        // 特殊键
        case KeyCodeWrapper.SLEEP: return "142";      // KEY_SLEEP
        case KeyCodeWrapper.ZOOM: return "174";       // KEY_ZOOM (可能不准确)

        case KeyCodeWrapper.NONE:
        default:
            // 对于未映射的键，返回 null 或空字符串
            Console.WriteLine($"警告: 未映射的 KeyCodeWrapper: {key} (值: {(int)key})");
            return null;
    }
}

    private static void ExecuteYDoToolCommand(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ydotool",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(1000); // 1秒超时
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行 ydotool 命令失败: {ex.Message}");
        }
    }
}

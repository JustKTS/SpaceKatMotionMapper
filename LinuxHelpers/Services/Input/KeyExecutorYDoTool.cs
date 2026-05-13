using System.Diagnostics;
using SpaceKat.Shared.Models;

namespace LinuxHelpers.Services.Input;

public static class KeyExecutorYDoTool
{
    private static bool? _isAvailable;

    private static bool IsAvailable()
    {
        if (_isAvailable.HasValue) return _isAvailable.Value;
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "ydotool",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit(1000);
            _isAvailable = process?.ExitCode == 0;
        }
        catch
        {
            _isAvailable = false;
        }
        return _isAvailable.Value;
    }

    private static readonly Dictionary<KeyCodeWrapper, string> KeyCodeMap = new()
    {
        // Letters
        { KeyCodeWrapper.A, "30" }, { KeyCodeWrapper.B, "48" }, { KeyCodeWrapper.C, "46" },
        { KeyCodeWrapper.D, "32" }, { KeyCodeWrapper.E, "18" }, { KeyCodeWrapper.F, "33" },
        { KeyCodeWrapper.G, "34" }, { KeyCodeWrapper.H, "35" }, { KeyCodeWrapper.I, "23" },
        { KeyCodeWrapper.J, "36" }, { KeyCodeWrapper.K, "37" }, { KeyCodeWrapper.L, "38" },
        { KeyCodeWrapper.M, "50" }, { KeyCodeWrapper.N, "49" }, { KeyCodeWrapper.O, "24" },
        { KeyCodeWrapper.P, "25" }, { KeyCodeWrapper.Q, "16" }, { KeyCodeWrapper.R, "19" },
        { KeyCodeWrapper.S, "31" }, { KeyCodeWrapper.T, "20" }, { KeyCodeWrapper.U, "22" },
        { KeyCodeWrapper.V, "47" }, { KeyCodeWrapper.W, "17" }, { KeyCodeWrapper.X, "45" },
        { KeyCodeWrapper.Y, "21" }, { KeyCodeWrapper.Z, "44" },

        // Digits
        { KeyCodeWrapper.NUM_0, "11" }, { KeyCodeWrapper.NUM_1, "2" }, { KeyCodeWrapper.NUM_2, "3" },
        { KeyCodeWrapper.NUM_3, "4" }, { KeyCodeWrapper.NUM_4, "5" }, { KeyCodeWrapper.NUM_5, "6" },
        { KeyCodeWrapper.NUM_6, "7" }, { KeyCodeWrapper.NUM_7, "8" }, { KeyCodeWrapper.NUM_8, "9" },
        { KeyCodeWrapper.NUM_9, "10" },

        // Function keys
        { KeyCodeWrapper.F1, "59" }, { KeyCodeWrapper.F2, "60" }, { KeyCodeWrapper.F3, "61" },
        { KeyCodeWrapper.F4, "62" }, { KeyCodeWrapper.F5, "63" }, { KeyCodeWrapper.F6, "64" },
        { KeyCodeWrapper.F7, "65" }, { KeyCodeWrapper.F8, "66" }, { KeyCodeWrapper.F9, "67" },
        { KeyCodeWrapper.F10, "68" }, { KeyCodeWrapper.F11, "87" }, { KeyCodeWrapper.F12, "88" },
        { KeyCodeWrapper.F13, "183" }, { KeyCodeWrapper.F14, "184" }, { KeyCodeWrapper.F15, "185" },
        { KeyCodeWrapper.F16, "186" }, { KeyCodeWrapper.F17, "187" }, { KeyCodeWrapper.F18, "188" },
        { KeyCodeWrapper.F19, "189" }, { KeyCodeWrapper.F20, "190" }, { KeyCodeWrapper.F21, "191" },
        { KeyCodeWrapper.F22, "192" }, { KeyCodeWrapper.F23, "193" }, { KeyCodeWrapper.F24, "194" },

        // Modifiers
        { KeyCodeWrapper.CONTROL, "29" }, { KeyCodeWrapper.LCONTROL, "29" }, { KeyCodeWrapper.RCONTROL, "97" },
        { KeyCodeWrapper.SHIFT, "42" }, { KeyCodeWrapper.LSHIFT, "42" }, { KeyCodeWrapper.RSHIFT, "54" },
        { KeyCodeWrapper.ALT, "56" }, { KeyCodeWrapper.LALT, "56" }, { KeyCodeWrapper.RALT, "100" },
        { KeyCodeWrapper.LWIN, "125" }, { KeyCodeWrapper.RWIN, "126" }, { KeyCodeWrapper.APPS, "127" },

        // Navigation
        { KeyCodeWrapper.TAB, "15" }, { KeyCodeWrapper.CAPITAL, "58" }, { KeyCodeWrapper.RETURN, "28" },
        { KeyCodeWrapper.ESCAPE, "1" }, { KeyCodeWrapper.SPACE, "57" }, { KeyCodeWrapper.BACK, "14" },
        { KeyCodeWrapper.DELETE, "111" }, { KeyCodeWrapper.INSERT, "110" }, { KeyCodeWrapper.HOME, "102" },
        { KeyCodeWrapper.END, "107" }, { KeyCodeWrapper.PRIOR, "104" }, { KeyCodeWrapper.NEXT, "109" },
        { KeyCodeWrapper.UP, "103" }, { KeyCodeWrapper.DOWN, "108" }, { KeyCodeWrapper.LEFT, "105" },
        { KeyCodeWrapper.RIGHT, "106" },

        // Numpad
        { KeyCodeWrapper.NUMPAD0, "82" }, { KeyCodeWrapper.NUMPAD1, "79" }, { KeyCodeWrapper.NUMPAD2, "80" },
        { KeyCodeWrapper.NUMPAD3, "81" }, { KeyCodeWrapper.NUMPAD4, "75" }, { KeyCodeWrapper.NUMPAD5, "76" },
        { KeyCodeWrapper.NUMPAD6, "77" }, { KeyCodeWrapper.NUMPAD7, "71" }, { KeyCodeWrapper.NUMPAD8, "72" },
        { KeyCodeWrapper.NUMPAD9, "73" }, { KeyCodeWrapper.MULTIPLY, "55" }, { KeyCodeWrapper.ADD, "78" },
        { KeyCodeWrapper.SUBTRACT, "74" }, { KeyCodeWrapper.DECIMAL, "83" }, { KeyCodeWrapper.DIVIDE, "98" },
        { KeyCodeWrapper.NUMLOCK, "69" }, { KeyCodeWrapper.SCROLL, "70" },

        // Symbols
        { KeyCodeWrapper.OEM_1, "39" }, { KeyCodeWrapper.OEM_PLUS, "13" }, { KeyCodeWrapper.OEM_COMMA, "51" },
        { KeyCodeWrapper.OEM_MINUS, "12" }, { KeyCodeWrapper.OEM_PERIOD, "52" }, { KeyCodeWrapper.OEM_2, "53" },
        { KeyCodeWrapper.OEM_3, "41" }, { KeyCodeWrapper.OEM_4, "26" }, { KeyCodeWrapper.OEM_5, "43" },
        { KeyCodeWrapper.OEM_6, "27" }, { KeyCodeWrapper.OEM_7, "40" }, { KeyCodeWrapper.OEM_102, "86" },

        // Misc
        { KeyCodeWrapper.PAUSE, "119" }, { KeyCodeWrapper.PRINT, "99" }, { KeyCodeWrapper.SNAPSHOT, "99" },
        { KeyCodeWrapper.HELP, "138" },

        // Media
        { KeyCodeWrapper.VOLUME_MUTE, "113" }, { KeyCodeWrapper.VOLUME_DOWN, "114" }, { KeyCodeWrapper.VOLUME_UP, "115" },
        { KeyCodeWrapper.MEDIA_NEXT_TRACK, "163" }, { KeyCodeWrapper.MEDIA_PREV_TRACK, "165" },
        { KeyCodeWrapper.MEDIA_STOP, "166" }, { KeyCodeWrapper.MEDIA_PLAY_PAUSE, "164" },

        // Browser
        { KeyCodeWrapper.BROWSER_BACK, "158" }, { KeyCodeWrapper.BROWSER_FORWARD, "159" },
        { KeyCodeWrapper.BROWSER_REFRESH, "173" }, { KeyCodeWrapper.BROWSER_STOP, "174" },
        { KeyCodeWrapper.BROWSER_SEARCH, "217" }, { KeyCodeWrapper.BROWSER_FAVORITES, "156" },
        { KeyCodeWrapper.BROWSER_HOME, "172" },

        { KeyCodeWrapper.SLEEP, "142" }, { KeyCodeWrapper.ZOOM, "174" },
    };

    private static readonly Dictionary<MouseButtonEnum, string> MouseKeyMap = new()
    {
        { MouseButtonEnum.LButton, "0xC0" },
        { MouseButtonEnum.RButton, "0xC1" },
        { MouseButtonEnum.MButton, "0xC2" },
    };

    public static bool ExecuteMouseAction(MouseActionConfig mouseActionConfig)
    {
        if (!IsAvailable()) return false;

        if (mouseActionConfig.Key == MouseButtonEnum.ScrollUp)
            return Run($"mousemove -w -- 0 {mouseActionConfig.Multiplier}");
        if (mouseActionConfig.Key == MouseButtonEnum.ScrollDown)
            return Run($"mousemove -w -- 0 -{mouseActionConfig.Multiplier}");
        if (mouseActionConfig.Key == MouseButtonEnum.ScrollLeft)
            return Run($"mousemove -w -- -{mouseActionConfig.Multiplier} 0");
        if (mouseActionConfig.Key == MouseButtonEnum.ScrollRight)
            return Run($"mousemove -w -- {mouseActionConfig.Multiplier} 0");

        if (!MouseKeyMap.TryGetValue(mouseActionConfig.Key, out var keyCode)) return false;

        return mouseActionConfig.PressMode switch
        {
            PressModeEnum.Click => Run($"click {keyCode}"),
            PressModeEnum.Press => Run($"mousedown {keyCode}"),
            PressModeEnum.Release => Run($"mouseup {keyCode}"),
            PressModeEnum.DoubleClick => Run($"click {keyCode}") && Sleep(50) && Run($"click {keyCode}"),
            _ => false
        };
    }

    public static void KeyBoardActionHandler(KeyBoardActionConfig keyBoardActionConfig)
    {
        if (!IsAvailable()) return;
        if (!KeyCodeMap.TryGetValue(keyBoardActionConfig.Key, out var keyCode)) return;

        switch (keyBoardActionConfig.PressMode)
        {
            case PressModeEnum.Click:
                Run($"key {keyCode}:1 {keyCode}:0");
                break;
            case PressModeEnum.Press:
                Run($"key {keyCode}:1");
                break;
            case PressModeEnum.Release:
                Run($"key {keyCode}:0");
                break;
        }
    }

    private static bool Run(string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ydotool",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            return process?.WaitForExit(1000) == true;
        }
        catch
        {
            return false;
        }
    }

    private static bool Sleep(int ms)
    {
        Thread.Sleep(ms);
        return true;
    }
}

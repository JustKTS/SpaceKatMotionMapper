using System.Collections.Frozen;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Helpers;

public static class VirtualKeyHelpers
{
    private static FrozenDictionary<string, KeyCodeWrapper> KeyDict { get; }
    public static IReadOnlyList<string> KeyNames { get; }

    static VirtualKeyHelpers()
    {
        var keyDict = new Dictionary<string, KeyCodeWrapper>();
        foreach (var key in KeyCodeWrapper.GetValues())
        {
            _ = keyDict.TryAdd(key.ToStringFast(useMetadataAttributes:true), key);
        }

        KeyDict = keyDict.ToFrozenDictionary();
        KeyNames = KeyCodeWrapper.GetNames()
            .ToList()
            .AsReadOnly();
    }

    public static KeyCodeWrapper Parse(string key)
    {
        return KeyDict.GetValueOrDefault(key, KeyCodeWrapper.NONE);
    }

}

public class VirtualKeyCodeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not KeyCodeWrapper keyCode ? string.Empty : keyCode.ToStringFast(useMetadataAttributes: true);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? KeyCodeWrapper.NONE : VirtualKeyHelpers.Parse(keyName);
    }
}

public static class KeyCodeWrapperExtension
{
    public static string GetWrappedName(this KeyCodeWrapper keyCode)
    {
        return keyCode.ToStringFast(useMetadataAttributes:true);
    }

    /// <summary>
    /// 跨平台：将 Avalonia.Input.Key 转换为 KeyCodeWrapper
    /// </summary>
    public static KeyCodeWrapper ToKeyCodeWrapper(this Key key)
    {
        // Avalonia Key 枚举名大多与 KeyCodeWrapper 一致，先尝试按名匹配
        if (Enum.TryParse<KeyCodeWrapper>(key.ToString(), out var result))
            return result;

        // Avalonia Key 与 KeyCodeWrapper 命名不一致的映射
        return key switch
        {
            Key.Tab => KeyCodeWrapper.TAB,
            Key.Enter => KeyCodeWrapper.RETURN,
            Key.Escape => KeyCodeWrapper.ESCAPE,
            Key.Back => KeyCodeWrapper.BACK,
            Key.CapsLock => KeyCodeWrapper.CAPITAL,
            Key.PageUp => KeyCodeWrapper.PRIOR,
            Key.PageDown => KeyCodeWrapper.NEXT,
            Key.LeftShift => KeyCodeWrapper.LSHIFT,
            Key.RightShift => KeyCodeWrapper.RSHIFT,
            Key.LeftCtrl => KeyCodeWrapper.LCONTROL,
            Key.RightCtrl => KeyCodeWrapper.RCONTROL,
            Key.LeftAlt => KeyCodeWrapper.LALT,
            Key.RightAlt => KeyCodeWrapper.RALT,
            Key.LWin => KeyCodeWrapper.LWIN,
            Key.RWin => KeyCodeWrapper.RWIN,
            Key.NumLock => KeyCodeWrapper.NUMLOCK,
            Key.Scroll => KeyCodeWrapper.SCROLL,
            Key.PrintScreen => KeyCodeWrapper.SNAPSHOT,
            Key.Pause => KeyCodeWrapper.PAUSE,
            Key.Insert => KeyCodeWrapper.INSERT,
            Key.Delete => KeyCodeWrapper.DELETE,
            Key.Home => KeyCodeWrapper.HOME,
            Key.End => KeyCodeWrapper.END,
            Key.Multiply => KeyCodeWrapper.MULTIPLY,
            Key.Add => KeyCodeWrapper.ADD,
            Key.Subtract => KeyCodeWrapper.SUBTRACT,
            Key.Decimal => KeyCodeWrapper.DECIMAL,
            Key.Divide => KeyCodeWrapper.DIVIDE,
            Key.OemTilde => KeyCodeWrapper.OEM_3,
            Key.OemCloseBrackets => KeyCodeWrapper.OEM_6,
            Key.OemOpenBrackets => KeyCodeWrapper.OEM_4,
            Key.OemPipe => KeyCodeWrapper.OEM_5,
            Key.OemSemicolon => KeyCodeWrapper.OEM_1,
            Key.OemQuotes => KeyCodeWrapper.OEM_7,
            Key.OemComma => KeyCodeWrapper.OEM_COMMA,
            Key.OemPeriod => KeyCodeWrapper.OEM_PERIOD,
            Key.OemQuestion => KeyCodeWrapper.OEM_2,
            Key.OemPlus => KeyCodeWrapper.OEM_PLUS,
            Key.OemMinus => KeyCodeWrapper.OEM_MINUS,
            Key.OemBackslash => KeyCodeWrapper.OEM_102,
            _ => KeyCodeWrapper.NONE
        };
    }
}

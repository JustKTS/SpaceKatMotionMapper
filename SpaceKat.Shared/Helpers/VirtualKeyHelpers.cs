using System.Collections.Frozen;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Win32.Input;
using SpaceKat.Shared.Models;
using WindowsInput;

namespace SpaceKat.Shared.Helpers;

public static class VirtualKeyHelpers
{
    private static FrozenDictionary<string, VirtualKeyCode> KeyDict { get; }
    public static IReadOnlyList<string> KeyNames { get; }

    static VirtualKeyHelpers()
    {
        var keyDict = new Dictionary<string, VirtualKeyCode>();
        foreach (var key in KeyCodeWrapperExtensions.GetValues())
        {
            _ = keyDict.TryAdd(key.ToStringFast(), (VirtualKeyCode)key);
        }

        KeyDict = keyDict.ToFrozenDictionary();
        KeyNames = KeyCodeWrapperExtensions.GetNames()
            .ToList()
            .AsReadOnly();
    }

    public static VirtualKeyCode Parse(string key)
    {
        return KeyDict.GetValueOrDefault(key, VirtualKeyCode.None);
    }
    
}

public class VirtualKeyCodeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not VirtualKeyCode keyCode ? string.Empty : keyCode.ToAvaloniaKey().ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string keyName ? VirtualKeyCode.None : VirtualKeyHelpers.Parse(keyName);
    }
}

public static class VirtualKeyCodeExtension
{
    public static Key ToAvaloniaKey(this VirtualKeyCode keyCode)
    {
        return KeyInterop.KeyFromVirtualKey((int)keyCode, 0);
    }

    public static string GetWrappedName(this VirtualKeyCode keyCode)
    {
        return ((KeyCodeWrapper)keyCode).ToStringFast();
    }
}

public static class AvaloniaKeyCodeExtension
{
    public static VirtualKeyCode ToVirtualKeyCode(this Key keyCode)
    {
        return (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(keyCode);
    }
}
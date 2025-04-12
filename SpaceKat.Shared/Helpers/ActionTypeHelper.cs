using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Helpers;

public static class ActionTypeHelper
{
    public static IReadOnlyList<string> ActionTypeNames { get; } = ActionTypeExtensions.GetValues()
        .Where(t => t is ActionType.Mouse or ActionType.KeyBoard).Select(t => t.ToStringFast()).ToList().AsReadOnly();

    public static ActionType Parse(string actionTypeStr)
    {
        var ret = ActionTypeExtensions.TryParse(actionTypeStr, out var actionType, ignoreCase: true,
            allowMatchingMetadataAttribute: true);
        return ret ? actionType : ActionType.KeyBoard;
    }

    private static readonly StreamGeometry KeyboardIconPath = StreamGeometry.Parse(
        "M19.7453641,5 C20.9880048,5 21.9953641,6.00735931 21.9953641,7.25 L21.9953641,16.754591 C21.9953641,17.9972317 20.9880048,19.004591 19.7453641,19.004591 L4.25,19.004591 C3.00735931,19.004591 2,17.9972317 2,16.754591 L2,7.25 C2,6.00735931 3.00735931,5 4.25,5 L19.7453641,5 Z M19.7453641,6.5 L4.25,6.5 C3.83578644,6.5 3.5,6.83578644 3.5,7.25 L3.5,16.754591 C3.5,17.1688046 3.83578644,17.504591 4.25,17.504591 L19.7453641,17.504591 C20.1595777,17.504591 20.4953641,17.1688046 20.4953641,16.754591 L20.4953641,7.25 C20.4953641,6.83578644 20.1595777,6.5 19.7453641,6.5 Z M6.75,14.5 L17.25,14.5 C17.6642136,14.5 18,14.8357864 18,15.25 C18,15.6296958 17.7178461,15.943491 17.3517706,15.9931534 L17.25,16 L6.75,16 C6.33578644,16 6,15.6642136 6,15.25 C6,14.8703042 6.28215388,14.556509 6.64822944,14.5068466 L6.75,14.5 L17.25,14.5 L6.75,14.5 Z M16.5,11 C17.0522847,11 17.5,11.4477153 17.5,12 C17.5,12.5522847 17.0522847,13 16.5,13 C15.9477153,13 15.5,12.5522847 15.5,12 C15.5,11.4477153 15.9477153,11 16.5,11 Z M10.5048752,11 C11.05716,11 11.5048752,11.4477153 11.5048752,12 C11.5048752,12.5522847 11.05716,13 10.5048752,13 C9.95259045,13 9.5048752,12.5522847 9.5048752,12 C9.5048752,11.4477153 9.95259045,11 10.5048752,11 Z M7.5048752,11 C8.05715995,11 8.5048752,11.4477153 8.5048752,12 C8.5048752,12.5522847 8.05715995,13 7.5048752,13 C6.95259045,13 6.5048752,12.5522847 6.5048752,12 C6.5048752,11.4477153 6.95259045,11 7.5048752,11 Z M13.5048752,11 C14.05716,11 14.5048752,11.4477153 14.5048752,12 C14.5048752,12.5522847 14.05716,13 13.5048752,13 C12.9525905,13 12.5048752,12.5522847 12.5048752,12 C12.5048752,11.4477153 12.9525905,11 13.5048752,11 Z M6,8 C6.55228475,8 7,8.44771525 7,9 C7,9.55228475 6.55228475,10 6,10 C5.44771525,10 5,9.55228475 5,9 C5,8.44771525 5.44771525,8 6,8 Z M8.9951248,8 C9.54740955,8 9.9951248,8.44771525 9.9951248,9 C9.9951248,9.55228475 9.54740955,10 8.9951248,10 C8.44284005,10 7.9951248,9.55228475 7.9951248,9 C7.9951248,8.44771525 8.44284005,8 8.9951248,8 Z M11.9951248,8 C12.5474095,8 12.9951248,8.44771525 12.9951248,9 C12.9951248,9.55228475 12.5474095,10 11.9951248,10 C11.44284,10 10.9951248,9.55228475 10.9951248,9 C10.9951248,8.44771525 11.44284,8 11.9951248,8 Z M14.9951248,8 C15.5474095,8 15.9951248,8.44771525 15.9951248,9 C15.9951248,9.55228475 15.5474095,10 14.9951248,10 C14.44284,10 13.9951248,9.55228475 13.9951248,9 C13.9951248,8.44771525 14.44284,8 14.9951248,8 Z M17.9951248,8 C18.5474095,8 18.9951248,8.44771525 18.9951248,9 C18.9951248,9.55228475 18.5474095,10 17.9951248,10 C17.44284,10 16.9951248,9.55228475 16.9951248,9 C16.9951248,8.44771525 17.44284,8 17.9951248,8 Z");

    private static readonly StreamGeometry MouseIconPath = StreamGeometry.Parse(
        "M4.25 9a7.75 7.75 0 1 1 15.5 0v6a7.75 7.75 0 1 1-15.5 0V9zm7-6.205C8.152 3.165 5.75 5.802 5.75 9v6a6.25 6.25 0 1 0 12.5 0V9c0-3.198-2.402-5.835-5.5-6.205v3.583a2.25 2.25 0 0 1 1.5 2.122v2a2.25 2.25 0 1 1-4.5 0v-2a2.25 2.25 0 0 1 1.5-2.122V2.795zM12 7.75a.75.75 0 0 0-.75.75v2a.75.75 0 1 0 1.5 0v-2a.75.75 0 0 0-.75-.75z");

    private static readonly StreamGeometry DelayIconPath = StreamGeometry.Parse(
        "M12 1a1.5 1.5 0 0 0 0 3 8 8 0 1 1-4.75 14.44 1.5 1.5 0 1 0-1.79 2.4A11 11 0 1 0 12 1ZM7.5 5.2a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3ZM5.5 7a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0Zm-3 6a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3ZM5 16.5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0Zm8.5-9a1.5 1.5 0 0 0-3 0V12c0 .4.16.78.44 1.06l3 3a1.5 1.5 0 0 0 2.12-2.12l-2.56-2.56V7.5Z");

    private static readonly StreamGeometry NullIconPath = new();


    public static StreamGeometry ToPathIconGeometry(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.KeyBoard => KeyboardIconPath,
            ActionType.Mouse => MouseIconPath,
            ActionType.Delay => DelayIconPath,
            _ => NullIconPath
        };
    }
}

public class ActionTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not ActionType actionType ? string.Empty : actionType.ToStringFast();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string actionTypeStr ? ActionType.KeyBoard : ActionTypeHelper.Parse(actionTypeStr);
    }
}

public class ActionTypeItemNamesConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string actionTypeStr) return VirtualKeyHelpers.KeyNames;
        var actionType = ActionTypeHelper.Parse(actionTypeStr);

        return actionType switch
        {
            ActionType.KeyBoard => VirtualKeyHelpers.KeyNames,
            ActionType.Mouse => MouseButtonHelper.ButtonNames,
            _ => VirtualKeyHelpers.KeyNames
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => null;
}

public class ActionTypeKeyOrButtonConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string actionTypeStr) return string.Empty;
        return actionTypeStr == "None" ? string.Empty : actionTypeStr;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string keyStr) return string.Empty;
        if (string.IsNullOrEmpty(keyStr) || string.IsNullOrWhiteSpace(keyStr)) return "None";

        try
        {
            var ret = MouseButtonEnumExtensions.TryParse(keyStr, out var button, ignoreCase: true,
                allowMatchingMetadataAttribute: true);
            if (ret)
            {
                return button.ToStringFast();
            }

            var keyCode = VirtualKeyHelpers.Parse(keyStr);
            return keyCode.GetWrappedName();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "None";
        }
    }
}

public class ActionTypeMouseToFalseConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ActionType actionType) return true;
        return actionType is not ActionType.Mouse;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
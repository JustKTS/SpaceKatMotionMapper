using Avalonia.Data.Converters;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System;
using System.Collections;

namespace SpaceKatMotionMapper.Helpers;

/// <summary>
/// 空集合转换为可见性转换器
/// 当集合为空时返回true（可见），否则返回false（不可见）
/// </summary>
public class EmptyCollectionToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IList collection)
        {
            return collection.Count == 0;
        }
        return true; // 默认可见
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

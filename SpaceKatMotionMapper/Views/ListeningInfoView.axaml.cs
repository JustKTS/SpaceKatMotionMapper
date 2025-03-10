using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class ListeningInfoView : UrsaView
{
    public ListeningInfoView()
    {
        DataContext = App.GetService<ListeningInfoViewModel>();
        InitializeComponent();
    }
}

public class KatButtonEnumToStrConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is KatButtonEnum buttonEnum)
        {
            return buttonEnum.ToStringFast();
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
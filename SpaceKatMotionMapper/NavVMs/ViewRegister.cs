using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace SpaceKatMotionMapper.NavVMs;

public class ViewRegister
{
    private readonly Dictionary<string, Type> _viewDict = [];
    public List<MenuItemViewModel> MenuItems { get; } = [];

    public void RegisterViewOfMenuItem<T>(string displayName, string icon)
        where T : Control
    {
        var viewType = typeof(T);
        var viewName = viewType.FullName;
        if (string.IsNullOrEmpty(viewName))
        {
            throw new ArgumentException($"'{nameof(viewName)}' cannot be null or empty.");
        }

        var item = new MenuItemViewModel(displayName, icon, viewName);
        MenuItems.Add(item);

        _viewDict.Add(viewType.FullName!, viewType);
    }

    public Control GetView(string key)
    {
        if (_viewDict.TryGetValue(key, out var type))
        {
            return App.GetRequiredView(type) as Control
                   ?? throw new Exception($"\"{key}\" have not been registered in App.axaml.cs");
        }
        else
        {
            throw new Exception($"View \"{key}\" not found");
        }
    }
}
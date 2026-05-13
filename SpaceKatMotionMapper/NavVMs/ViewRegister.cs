using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Serilog;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.NavVMs;

public class ViewRegister : IViewRegister
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ViewRegister>();

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
            var sw = Stopwatch.StartNew();
            var view = App.GetService(type) as Control
                       ?? throw new Exception($"\"{key}\" have not been registered in App.axaml.cs");
            sw.Stop();
            if (sw.Elapsed.TotalMilliseconds > 1)
                Log.Information("ViewRegister.GetView({Key}) => {Type} took {Ms:F2}ms", key, type.Name, sw.Elapsed.TotalMilliseconds);
            return view;
        }
        else
        {
            throw new Exception($"View \"{key}\" not found");
        }
    }
}

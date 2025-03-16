using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class CleanOldConfigsWindow : UrsaWindow
{
    public CleanOldConfigsWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(SpaceKatMotionMapper));
        var filenames = Directory.GetFiles(path, "*.json").ToList();
        var path2 = Path.Combine(path, "CustomConfigs");
        filenames.AddRange(Directory.GetFiles(path2, "*.json"));
        foreach (var filename in filenames)
        {
            File.Delete(filename);
        }
        Close();
    }

    private void Button2_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
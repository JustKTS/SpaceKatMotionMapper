using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SpaceKatMotionMapper.Helpers;

public static class TopLevelHelper
{
    public static TopLevel GetTopLevel() {
        var control = Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime single => single.MainView,
            _ => null
        } ?? throw new Exception(
            "TopLevel not found. Please do not request this Helper before the main page is loaded.");

        return TopLevel.GetTopLevel(control) ??
               throw new Exception(
                   "TopLevel not found. Please do not request this Helper before the main page is loaded.");
    }
}
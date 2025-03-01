using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class TopLevelHelper : ITopLevelHelper
{
    public TopLevel GetTopLevel() {
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
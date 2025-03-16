using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SpaceKatMotionMapper.Views;

namespace SpaceKatMotionMapper.NavVMs;

public partial class NavViewModel : ObservableRecipient
{
    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = [];

    [ObservableProperty] private object? _content;

    private readonly ViewRegister _viewRegister;


    public NavViewModel(ViewRegister viewRegister)
    {
        _viewRegister = viewRegister;
        _viewRegister.RegisterViewOfMenuItem<MainView>("主页面", "fa-solid fa-home-alt");
        _viewRegister.RegisterViewOfMenuItem<SettingsView>("设置页面", "fa-solid fa-list");
        _viewRegister.MenuItems.ForEach(MenuItems.Add);

        WeakReferenceMessenger.Default.Register<NavViewModel, string>(this, OnNavigation);
    }

    public void OnNavigation(NavViewModel vm, string s)
    {
        try
        {
            Content = _viewRegister.GetView(s);
        }
        catch (Exception e)
        {
            Content = null;
            var window = new CleanOldConfigsWindow();
            window.ShowDialog(App.GetRequiredService<MainWindow>());
        }
        
    }
}
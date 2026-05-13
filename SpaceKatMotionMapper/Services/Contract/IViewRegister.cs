using System.Collections.Generic;
using Avalonia.Controls;
using SpaceKatMotionMapper.NavVMs;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IViewRegister
{
    List<MenuItemViewModel> MenuItems { get; }
    void RegisterViewOfMenuItem<T>(string displayName, string icon) where T : Control;
    Control GetView(string key);
}

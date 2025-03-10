using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace SpaceKatMotionMapper.NavVMs;

public class MenuItemViewModel
{
    public string MenuHeader { get; set; }
    public string MenuIconName { get; set; }
    public string Key { get; set; }

    public bool IsSeparator { get; set; }
    public ObservableCollection<MenuItemViewModel> Children { get; set; } = [];

    public ICommand ActivateCommand { get; set; }


    public MenuItemViewModel(string menuHeader, string menuIconName, string key)
    {
        MenuHeader = menuHeader;
        MenuIconName = menuIconName;
        Key = key;
        ActivateCommand = new RelayCommand(OnActivate);
    }

    private void OnActivate()
    {
        if (IsSeparator)
            return;
        WeakReferenceMessenger.Default.Send(Key);
    }
}
using Avalonia.Controls;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        DataContext = App.GetRequiredService<MainViewModel>();
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (App.GetRequiredService<MetaKeyPresetService>().IsFirstStart())
            {
                Dialog.ShowStandardAsync<FirstDownloadPresetsView, FirstDownloadPresetsViewModel>(
                    App.GetRequiredService<FirstDownloadPresetsViewModel>(), options: new DialogOptions
                    {
                        StartupLocation = WindowStartupLocation.CenterOwner,
                        Mode = DialogMode.Info,
                        IsCloseButtonVisible = false,
                        ShowInTaskBar = false,
                        CanDragMove = false,
                        CanResize = false,
                        Button = DialogButton.None
                    });
            }
        };
    }
}
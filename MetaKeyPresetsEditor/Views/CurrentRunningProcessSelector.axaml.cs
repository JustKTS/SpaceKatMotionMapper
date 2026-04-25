using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MetaKeyPresetsEditor.Helpers;
using MetaKeyPresetsEditor.ViewModels;

namespace MetaKeyPresetsEditor.Views;

public partial class CurrentRunningProcessSelector : UserControl
{
    public CurrentRunningProcessSelector()
    {
        DataContext = null;
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        var vm = DIHelper.GetServiceProvider().GetRequiredService<CurrentRunningProcessSelectorViewModel>();
        DataContext = vm;
        LoadingContainerA.IsLoading = true;
        vm.UpdateForeProcessInfosAsync().ContinueWith(_ =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                LoadingContainerA.IsLoading = false;
            });
        });
    }
}
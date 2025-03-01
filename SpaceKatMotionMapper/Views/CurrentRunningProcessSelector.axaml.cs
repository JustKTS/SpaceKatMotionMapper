using Avalonia.LogicalTree;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class CurrentRunningProcessSelector : UrsaView
{
    public CurrentRunningProcessSelector()
    {
        DataContext = null;
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (Parent?.DataContext is not KatActionConfigViewModel kbVm)
        {
            return;
        }
        var vm = new CurrentRunningProcessSelectorViewModel(kbVm);
        DataContext = vm;
        vm.UpdateForeProcessInfos();
    }
}
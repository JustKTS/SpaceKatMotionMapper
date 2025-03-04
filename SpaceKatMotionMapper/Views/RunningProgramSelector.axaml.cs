using Avalonia.LogicalTree;
using SpaceKatMotionMapper.ViewModels;
using Ursa.Controls;

namespace SpaceKatMotionMapper.Views;

public partial class RunningProgramSelector : UrsaView
{
    public RunningProgramSelector()
    {
        DataContext = null;
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (Parent?.DataContext is not AutoDisableViewModel adVm)
        {
            return;
        }
        var vm = new RunningProgramSelectorViewModel(adVm);
        DataContext = vm;
        vm.UpdateForeProcessInfos();
    }
}
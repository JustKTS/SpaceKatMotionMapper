using Avalonia.Controls;
using Avalonia.LogicalTree;
using Serilog;
using SpaceKat.Shared.ViewModels;

namespace SpaceKat.Shared.Views;

public partial class RunningProgramSelector : UserControl
{
    public RunningProgramSelector()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        // 每次附加到逻辑树时都尝试加载数据
        // 使用异步延迟，确保 DataContext 已经设置
        _ = LoadDataWhenReadyAsync();
    }

    private async Task LoadDataWhenReadyAsync()
    {
        // 等待一小段时间，确保 DataContext 已经被设置
        await Task.Delay(50);

        if (DataContext is RunningProgramSelectorViewModel vm)
        {
            await UpdateDataAsync(vm);
        }
    }

    private async Task UpdateDataAsync(RunningProgramSelectorViewModel vm)
    {
        try
        {
            await vm.UpdateForeProcessInfosAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{View}] Failed to load window list", nameof(RunningProgramSelector));
        }
    }
}

﻿using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using MetaKeyPresetsEditor.ViewModels;
using Ursa.Controls;

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
        var vm = new CurrentRunningProcessSelectorViewModel();
        DataContext = vm;
        LoadingContainerA.IsLoading = true;
        vm.UpdateForeProcessInfosAsync().ContinueWith(t =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                LoadingContainerA.IsLoading = false;
            });
        });
    }
}
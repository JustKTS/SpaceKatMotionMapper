using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Helpers;

namespace SpaceKatMotionMapper.ViewModels;

public partial class CurrentRunningProcessSelectorViewModel(KatActionConfigViewModel parent) : ViewModelBase
{
#if DEBUG
    public CurrentRunningProcessSelectorViewModel() : this(null!)
    {
    }
#endif
    
    public ObservableCollection<ForeProgramInfo> ForeProcessInfos { get; } = [];
    
    [ObservableProperty]
    private ForeProgramInfo? _selectedFpInfo;

    partial void OnSelectedFpInfoChanged(ForeProgramInfo? value)
    {
        if (value is null) return;
        parent.ProcessPath = value.ProcessFileAddress;
    }


    public void UpdateForeProcessInfos()
    {
        ForeProcessInfos.Clear();
        var fpInfos = CurrentForeProgramHelper.FindAll();
        fpInfos.Iter(ForeProcessInfos.Add);
    }

    [RelayCommand]
    private void SelectProcess()
    {
        parent.SelectProcessPathCommand.Execute(null);
    }
}
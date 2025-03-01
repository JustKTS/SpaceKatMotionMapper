using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatActionsWithModeViewModel: ViewModelBase
{
    [ObservableProperty] private int _modeNum;
    public ObservableCollection<KatActionViewModel> KatActions { get; set; }
    public KatActionConfigViewModel Parent { get; }
    
#if DEBUG
    public KatActionsWithModeViewModel():this(null!, 0)
    {
        
    }
#endif

    public KatActionsWithModeViewModel(KatActionConfigViewModel parent, int modeNum)
    {
        Parent = parent;
        ModeNum = modeNum;
        KatActions = [new KatActionViewModel(this, ModeNum)];
    }
    
    [RelayCommand]
    private void AddKatActionConfig()
    {
        KatActions.Add(new KatActionViewModel(this, ModeNum));
    }

    [RelayCommand]
    private void RemoveKatActionConfig(int index)
    {
        if (index < 0) return;
        KatActions.RemoveAt(index);
        if (KatActions.Count == 0)
        {
            KatActions.Add(new KatActionViewModel(this, ModeNum));
        }
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        var index = Parent.KatActionsWithMode.IndexOf(this);
        Parent.RemoveKatActionsWithModeCommand.Execute(index);
    }
    
    [RelayCommand]
    private void AddOther()
    {
        Parent.AddKatActionsWithModeCommand.Execute(null);
    }
    
}
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatMotionsWithModeViewModel: ViewModelBase
{
    [ObservableProperty] private int _modeNum;
    public ObservableCollection<KatMotionViewModel> KatMotions { get; set; }
    public KatMotionConfigViewModel Parent { get; }

    public bool IsAvailable => CheckAvailable();

    private bool CheckAvailable()
    {
        return KatMotions.All(x => x.IsAvailable);
    }
    
#if DEBUG
    public KatMotionsWithModeViewModel():this(null!, 0)
    {
        
    }
#endif

    public KatMotionsWithModeViewModel(KatMotionConfigViewModel parent, int modeNum)
    {
        Parent = parent;
        ModeNum = modeNum;
        KatMotions = [];
        KatMotions.CollectionChanged += (_, e) =>
        {
            // 处理新增项：订阅PropertyChanged
            if (e.NewItems != null)
            {
                foreach (KatMotionViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }
        
            // 处理移除项：取消订阅避免内存泄漏
            if (e.OldItems == null) return;
        
            foreach (KatMotionViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        };
        KatMotions.Add(new KatMotionViewModel(this, ModeNum));
    }
    
    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(KatMotionViewModel.IsAvailable))
        {
            OnPropertyChanged(nameof(IsAvailable));
        }
    }
    
    [RelayCommand]
    private void AddKatMotionConfig()
    {
        KatMotions.Add(new KatMotionViewModel(this, ModeNum));
        OnPropertyChanged(nameof(IsAvailable));
    }

    [RelayCommand]
    private void RemoveKatMotionConfig(int index)
    {
        if (index < 0) return;
        KatMotions.RemoveAt(index);
        if (KatMotions.Count == 0)
        {
            KatMotions.Add(new KatMotionViewModel(this, ModeNum));
        }
        OnPropertyChanged(nameof(IsAvailable));
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        var index = Parent.KatMotionsWithMode.IndexOf(this);
        Parent.RemoveKatMotionsWithModeCommand.Execute(index);
    }
    
    [RelayCommand]
    private void AddOther()
    {
        Parent.AddKatMotionsWithModeCommand.Execute(null);
    }
    
}
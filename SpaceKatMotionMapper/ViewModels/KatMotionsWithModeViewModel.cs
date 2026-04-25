using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Views;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels;

public partial class KatMotionsWithModeViewModel: ViewModelBase
{
    [ObservableProperty] private int _modeNum;
    [ObservableProperty] private bool _hasAvailableMotions = true;
    [ObservableProperty] private int _selectedMotionGroupIndex = -1;
    public ObservableCollection<KatMotionGroupViewModel> KatMotionGroups { get; set; }
    public KatMotionConfigViewModel Parent { get; }

    public bool IsAvailable => CheckAvailable();

    private bool CheckAvailable()
    {
        return KatMotionGroups.All(x => x.IsAvailable);
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
        KatMotionGroups = [];
        KatMotionGroups.CollectionChanged += (_, e) =>
        {
            // 处理新增项：订阅PropertyChanged
            if (e.NewItems != null)
            {
                foreach (KatMotionGroupViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }

            // 处理移除项：取消订阅避免内存泄漏
            if (e.OldItems == null) return;

            foreach (KatMotionGroupViewModel item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }

            // 更新可用运动状态
            UpdateHasAvailableMotions();

            // 通知属性更新以触发空状态可见性转换
            OnPropertyChanged(nameof(KatMotionGroups));
        };
    }

    private void UpdateHasAvailableMotions()
    {
        var allMotions = System.Enum.GetValues<KatMotionEnum>()
            .Where(m => m != KatMotionEnum.Null && m != KatMotionEnum.Stable);

        var existing = KatMotionGroups.Select(g => g.KatMotion)
            .Where(m => m != KatMotionEnum.Null && m != KatMotionEnum.Stable);

        HasAvailableMotions = allMotions.Except(existing).Any();
    }

    private void ChildPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(KatMotionGroupViewModel.IsAvailable))
        {
            OnPropertyChanged(nameof(IsAvailable));
        }
    }
    
    [RelayCommand]
    private async Task ShowAddMotionDialogAsync()
    {
        // 获取已选择的运动类型
        var existingMotions = KatMotionGroups.Select(g => g.KatMotion).ToList();

        // 创建弹窗ViewModel
        var dialogViewModel = new KatMotionSelectDialogViewModel(existingMotions);

        var ret = await ShowMotionSelectDialogAsync(dialogViewModel);

        // 如果用户选择了有效的运动类型
        if (ret is { } motion)
        {
            KatMotionGroups.Add(new KatMotionGroupViewModel(this, motion));
            SelectedMotionGroupIndex = KatMotionGroups.Count - 1; // 自动切换到新选项卡
            UpdateHasAvailableMotions();
            OnPropertyChanged(nameof(IsAvailable));
            OnPropertyChanged(nameof(KatMotionGroups)); // 确保空状态绑定更新
        }
   
    }

    private static Task<KatMotionEnum?> ShowMotionSelectDialogAsync(KatMotionSelectDialogViewModel dialogViewModel)
    {
        return OverlayDialog.ShowCustomAsync<KatMotionSelectDialog, KatMotionSelectDialogViewModel, KatMotionEnum?>(
            dialogViewModel,
            KatMotionGroupConfigWindow.LocalHost,
            KatMotionSelectDialogViewModel.OverlayDialogOptions);
    }

    [RelayCommand]
    private void RemoveKatMotionGroup(int index)
    {
        if (index < 0) return;
        KatMotionGroups.RemoveAt(index);
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
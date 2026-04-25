using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaceKatHIDWrapper.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Irihi.Avalonia.Shared.Contracts;
using Ursa.Controls;

namespace SpaceKatMotionMapper.ViewModels
{
    /// <summary>
    /// 运动选项包装类，用于UI显示
    /// </summary>
    public partial class MotionOption : ObservableObject
    {
        [ObservableProperty] private KatMotionEnum _motion;
        [ObservableProperty] private string _displayName;

        public MotionOption(KatMotionEnum motion)
        {
            Motion = motion;
            DisplayName = motion.ToStringFast(useMetadataAttributes: true);
        }
    }

    /// <summary>
    /// 运动选择弹窗的ViewModel
    /// </summary>
    public partial class KatMotionSelectDialogViewModel : ViewModelBase, IDialogContext
    {
        public static readonly DialogOptions DialogOptions = new()
        {
            Button = DialogButton.None,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = true,
            Mode = DialogMode.None
        };

        public static readonly OverlayDialogOptions OverlayDialogOptions = new()
        {
            Buttons = DialogButton.None,
            HorizontalAnchor = HorizontalPosition.Center,
            VerticalAnchor = VerticalPosition.Center,
            CanDragMove = false,
            CanLightDismiss = true,
            CanResize = false,
            FullScreen = false,
            IsCloseButtonVisible = true,
            Mode = DialogMode.None
        };

        [ObservableProperty] private MotionOption? _selectedMotion;

        /// <summary>
        /// 可选的运动类型列表（已过滤掉已选择的）
        /// </summary>
        public ObservableCollection<MotionOption> AvailableMotions { get; }

        public KatMotionSelectDialogViewModel(IEnumerable<KatMotionEnum> existingMotions)
        {
            // 过滤掉已选择的运动类型，排除Null和Stable
            var allMotions = System.Enum.GetValues<KatMotionEnum>()
                .Cast<KatMotionEnum>()
                .Where(m => m != KatMotionEnum.Null && m != KatMotionEnum.Stable);

            var existing = existingMotions.Where(m => m != KatMotionEnum.Null && m != KatMotionEnum.Stable);
            var available = allMotions.Except(existing);

            AvailableMotions = new ObservableCollection<MotionOption>(
                available.Select(m => new MotionOption(m))
            );

            SelectedMotion = AvailableMotions.FirstOrDefault();
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            // 关闭弹窗并返回选择结果
            RequestClose?.Invoke(this, SelectedMotion?.Motion);
        }

        /// <summary>
        /// 直接选择运动类型并关闭对话框
        /// </summary>
        [RelayCommand]
        private void SelectMotion(KatMotionEnum motion)
        {
            // 立即关闭弹窗并返回选中的运动
            RequestClose?.Invoke(this, motion);
        }

        public void Close()
        {
            RequestClose?.Invoke(this, null);
        }

        public event EventHandler<object?>? RequestClose;
    }
}
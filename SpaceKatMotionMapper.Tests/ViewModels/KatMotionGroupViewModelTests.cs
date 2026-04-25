using System.Reflection;
using TUnit.Assertions;
using SpaceKatMotionMapper.Tests.Helpers;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Tests.ViewModels;

public class KatMotionGroupViewModelTests
{
    [Test]
    public async Task FirstConfigMode_WhenInternalRollbackUpdate_ShouldNotStartConfirmFlow()
    {
        // Arrange
        var configVm = ViewModelTestHelpers.CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(configVm, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationYPositive);

        SetPrivateField(groupVm, "_isInternalFirstConfigModeUpdate", true);

        // Act: simulate internal rollback write-back path
        groupVm.FirstConfigMode = KatConfigModeEnum.Advanced;

        // Assert: confirm-flow reentrancy guard never gets enabled
        var isModeChangeConfirming = (bool)GetPrivateField(groupVm, "_isModeChangeConfirming");
        await Assert.That(isModeChangeConfirming).IsFalse();
        await Assert.That(groupVm.FirstConfigMode).IsEqualTo(KatConfigModeEnum.Advanced);
    }

    [Test]
    public async Task OnConfigsCollectionChanged_WhenConfigAdded_ShouldFirePropertyNotifications()
    {
        // Arrange
        var configVm = ViewModelTestHelpers.CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(configVm, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationYPositive);
        groupVm.Configs.Clear();

        var receivedNotifications = new System.Collections.Generic.List<string>();
        groupVm.PropertyChanged += (_, e) => receivedNotifications.Add(e.PropertyName!);

        var newConfig = new KatMotionViewModel(groupVm, 0)
        {
            KatMotion = KatMotionEnum.TranslationYPositive,
            ConfigMode = KatConfigModeEnum.Advanced
        };

        // Act
        groupVm.Configs.Add(newConfig);

        // Assert
        await Assert.That(receivedNotifications).Contains(nameof(KatMotionGroupViewModel.IsSingleActionMode));
        await Assert.That(receivedNotifications).Contains(nameof(KatMotionGroupViewModel.CanAddMoreConfigs));
        await Assert.That(receivedNotifications).Contains(nameof(KatMotionGroupViewModel.FirstConfigMode));
        await Assert.That(receivedNotifications).Contains(nameof(KatMotionGroupViewModel.IsAvailable));
    }

    private static object GetPrivateField(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(target)!;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}


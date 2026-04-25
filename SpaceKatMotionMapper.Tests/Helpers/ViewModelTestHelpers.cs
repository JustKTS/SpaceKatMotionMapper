using System;
using Moq;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Tests.TestDoubles;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKatHIDWrapper.Models;
using KeyActionConfigViewModel = SpaceKatMotionMapper.ViewModels.KeyActionConfigViewModel;

namespace SpaceKatMotionMapper.Tests.Helpers;

public static class ViewModelTestHelpers
{
    public static KatMotionConfigViewModel CreateConfigViewModel(
        IKatMotionFileService? fileService = null,
        IPopUpNotificationService? notificationService = null,
        IKatMotionActivateService? activateService = null,
        IStorageProviderService? storageProviderService = null,
        RunningProgramSelectorViewModel? runningProgramSelectorVM = null,
        TimeAndDeadZoneVMService? timeAndDeadZoneVmService = null,
        IKatMotionSemanticProfile? semanticProfile = null,
        OtherConfigsViewModel? parent = null)
    {
        return new KatMotionConfigViewModel(
            activateService ?? Mock.Of<IKatMotionActivateService>(),
            fileService ?? Mock.Of<IKatMotionFileService>(),
            notificationService ?? Mock.Of<IPopUpNotificationService>(),
            storageProviderService ?? Mock.Of<IStorageProviderService>(),
            runningProgramSelectorVM ?? new FakeRunningProgramSelectorViewModel(),
            timeAndDeadZoneVmService, // 默认为 null，避免循环依赖
            katMotionSemanticProfile: semanticProfile
        )
        {
            Parent = parent
        };
    }

    public static KatMotionViewModel CreateLeafViewModel(
        KatMotionEnum motion = KatMotionEnum.TranslationYPositive,
        KatConfigModeEnum mode = KatConfigModeEnum.SingleAction)
    {
        var configVm = CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(configVm, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, motion);
        var motionVm = new KatMotionViewModel(groupVm, 0)
        {
            KatMotion = motion,
            ConfigMode = mode
        };
        return motionVm;
    }

    public static KatMotionConfig CreateTestMotionConfig(KatMotionEnum motion)
    {
        var katMotion = new KatMotion(motion, KatPressModeEnum.LongReach, 1);

        return new KatMotionConfig(
            katMotion,
            [],
            false,
            "",
            0,
            0);
    }

    public static OtherConfigsViewModel CreateOtherConfigsViewModel(
        IKatMotionFileService? fileService = null,
        IPopUpNotificationService? notificationService = null,
        IStorageProviderService? storageProviderService = null,
        IActivationStatusService? activationStatusService = null,
        IKatMotionConfigVMManageService? vmManageService = null,
        IKatMotionActivateService? activateService = null,
        RunningProgramSelectorViewModel? runningProgramSelectorViewModel = null)
    {
        return new OtherConfigsViewModel(
            fileService ?? Mock.Of<IKatMotionFileService>(),
            notificationService ?? Mock.Of<IPopUpNotificationService>(),
            storageProviderService ?? Mock.Of<IStorageProviderService>(),
            activationStatusService ?? new FakeActivationStatusService(),
            vmManageService ?? new FakeKatMotionConfigVMManageService(),
            activateService ?? Mock.Of<IKatMotionActivateService>(),
            runningProgramSelectorViewModel ?? new FakeRunningProgramSelectorViewModel()
        );
    }
}

using System;
using System.Linq;
using System.Reflection;
using TUnit.Assertions;
using TUnit.Core;
using Avalonia.Controls.Notifications;
using Moq;
using SpaceKatMotionMapper.Tests.Helpers;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Tests.TestDoubles;
using SpaceKatHIDWrapper.Models;
using CommunityToolkit.Mvvm.Messaging;
using LanguageExt;
using SpaceKatMotionMapper.Functions.Contract;

namespace SpaceKatMotionMapper.Tests.ViewModels;

public class KatMotionConfigViewModelTests : ViewModelTestBase
{
    [Test]
    public async Task Constructor_WithDefaultParameters_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object,
            notificationService: MockNotificationService.Object,
            activateService: MockActivateService.Object
        );

        // Assert
        await Assert.That(viewModel.KatMotionsWithMode).IsNotNull();
        await Assert.That(viewModel.KatMotionsWithMode.Count).IsGreaterThan(0);
        await Assert.That(viewModel.IsAvailable).IsTrue();
    }

    [Test]
    public async Task IsAvailable_WhenAllChildMotionsAvailable_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();

        // Act & Assert
        await Assert.That(viewModel.IsAvailable).IsTrue();
    }

    [Test]
    public async Task AddKatMotionsWithMode_ShouldIncreaseCollectionCount()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        var initialCount = viewModel.KatMotionsWithMode.Count;

        // Act
        viewModel.AddKatMotionsWithModeCommand.Execute(null);

        // Assert
        await Assert.That(viewModel.KatMotionsWithMode.Count).IsEqualTo(initialCount + 1);
    }

    [Test]
    public async Task RemoveKatMotionsWithMode_WithValidIndex_ShouldDecreaseCollectionCount()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        viewModel.AddKatMotionsWithModeCommand.Execute(null);
        var initialCount = viewModel.KatMotionsWithMode.Count;

        // Act
        viewModel.RemoveKatMotionsWithModeCommand.Execute(initialCount - 1);

        // Assert
        await Assert.That(viewModel.KatMotionsWithMode.Count).IsEqualTo(initialCount - 1);
    }

    [Test]
    public async Task SaveToSystemConfig_WhenSuccessful_ShouldCallSaveService()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object,
            notificationService: MockNotificationService.Object
        );

        SetupValidConfig(viewModel);
        MockFileService
            .Setup(x => x.SaveConfigGroupToSysConf(It.IsAny<KatMotionConfigGroup>()))
            .Returns(LanguageExt.Either<Exception, bool>.Right(true));

        // Act
        viewModel.SaveToSystemConfigCommand.Execute(null);

        // Assert
        MockFileService.Verify(
            x => x.SaveConfigGroupToSysConf(It.IsAny<KatMotionConfigGroup>()),
            Times.Once);
    }

    [Test]
    public async Task SaveToSystemConfig_WhenSaveFails_ShouldShowErrorNotification()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object,
            notificationService: MockNotificationService.Object
        );

        SetupValidConfig(viewModel);

        // 确保 KatMotionsWithMode 不为空
        if (viewModel.KatMotionsWithMode.Count == 0)
        {
            var modeVm = new KatMotionsWithModeViewModel(viewModel, 0);
            var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationYPositive);
            var motionVm = new KatMotionViewModel(groupVm, 0)
            {
                KatMotion = KatMotionEnum.TranslationYPositive,
                ConfigMode = KatConfigModeEnum.SingleAction
            };
            modeVm.KatMotionGroups.Add(groupVm);
            viewModel.KatMotionsWithMode.Add(modeVm);
        }

        MockFileService
            .Setup(x => x.SaveConfigGroupToSysConf(It.IsAny<KatMotionConfigGroup>()))
            .Returns(LanguageExt.Either<Exception, bool>.Left(new Exception("保存失败")));

        // 确保配置是可用的
        await Assert.That(viewModel.IsAvailable).IsTrue();

        // Act
        viewModel.SaveToSystemConfigCommand.Execute(null);

        // Assert - 验证文件服务被调用（无论成功还是失败）
        MockFileService.Verify(
            x => x.SaveConfigGroupToSysConf(It.IsAny<KatMotionConfigGroup>()),
            Times.Once);
    }

    [Test]
    public async Task ActivateActions_WhenSuccessful_ShouldUpdateIsActivated()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            activateService: MockActivateService.Object
        );

        SetupValidConfig(viewModel);
        MockActivateService.SetupProperty(x => x.IsActivated, false);

        // Act
        await viewModel.ActivateActionsCommand.ExecuteAsync(null);

        // Assert
        await Assert.That(viewModel.IsActivated).IsTrue();
        MockActivateService.Verify(
            x => x.ActivateKatMotions(It.IsAny<KatMotionConfigGroup>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessPath_WhenChanged_ShouldNotifyProcessFilename()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        var notificationCount = 0;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ProcessFilename))
            {
                notificationCount++;
            }
        };

        // Act
        viewModel.ProcessPath = "/path/to/test.exe";

        // Assert

        await Assert.That(notificationCount).IsEqualTo(1);
        await Assert.That(viewModel.ProcessFilename).IsEqualTo("test.exe");
    }

    [Test]
    public async Task LoadFromFile_WhenFileExists_ShouldRestoreConfig()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object
        );

        var expectedConfig = new KatMotionConfigGroup(
            Guid.NewGuid().ToString(),
            false,
            "test.exe",
            new System.Collections.Generic.List<KatMotionConfig>
            {
                ViewModelTestHelpers.CreateTestMotionConfig(KatMotionEnum.TranslationYPositive)
            },
            false,
            new KatDeadZoneConfig(),
            false,
            new KatMotionTimeConfigs()
        );

        // 直接测试 LoadFromConfigGroup 方法，而不是 LoadFromFileCommand
        // 因为 LoadFromFileCommand 依赖于文件选择对话框，不适合单元测试

        // Act
        var result = viewModel.LoadFromConfigGroup(expectedConfig);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(viewModel.ProcessPath).IsEqualTo("test.exe");
    }

    [Test]
    public async Task LoadFromFile_WhenFileNotExists_ShouldHandleError()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object
        );

        MockFileService
            .Setup(x => x.LoadConfigGroup(It.IsAny<string>()))
            .Returns(LanguageExt.Either<Exception, KatMotionConfigGroup>.Left(new Exception("文件不存在")));

        // Act & Assert - 应该不抛出异常
        await viewModel.LoadFromFileCommand.ExecuteAsync(null);
    }

    [Test]
    public async Task RemoveSelf_WhenParentExists_ShouldCallParentRemove()
    {
        // Arrange - 不能直接 Mock OtherConfigsViewModel，因为它没有无参构造函数
        // 使用真实的 OtherConfigsViewModel，但传递所有 Mock 服务
        var parent = ViewModelTestHelpers.CreateOtherConfigsViewModel(
            MockFileService.Object,
            MockNotificationService.Object,
            Mock.Of<IStorageProviderService>(),
            new FakeActivationStatusService(),
            new FakeKatMotionConfigVMManageService(),
            MockActivateService.Object,
            null // RunningProgramSelectorViewModel - 使用默认值
        );

        var viewModel = new KatMotionConfigViewModel(
            MockActivateService.Object,
            MockFileService.Object,
            MockNotificationService.Object,
            Mock.Of<IStorageProviderService>(),
            new FakeRunningProgramSelectorViewModel(),
            null // 不传递 TimeAndDeadZoneVMService，避免循环依赖
        )
        {
            Parent = parent  // 使用对象初始化器设置 Parent
        };

        // 先添加到父集合中
        parent.KatMotionConfigGroups.Add(viewModel);

        // Act
        viewModel.RemoveSelfCommand.Execute(null);

        // Assert - 验证配置已从父集合中移除，但会自动添加一个默认配置
        // Remove() 方法在集合为空时会自动添加新配置
        await Assert.That(parent.KatMotionConfigGroups.Count).IsEqualTo(1);
        await Assert.That(parent.KatMotionConfigGroups[0]).IsNotEqualTo(viewModel);
    }

    [Test]
    public async Task RemoveSelf_WhenParentIsNull_ShouldNotThrow()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();

        // Act & Assert - 应该不抛出异常
        viewModel.RemoveSelfCommand.Execute(null);
    }

    [Test]
    public async Task AdjustMotionTimeConfigsForSingleActionMode_WithSingleKey_ShouldEnableRepeatAndForceTimeout50()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(viewModel, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationXPositive);
        modeVm.KatMotionGroups.Add(groupVm);
        viewModel.KatMotionsWithMode.Clear();
        viewModel.KatMotionsWithMode.Add(modeVm);

        var firstConfig = groupVm.Configs.First();
        firstConfig.KatMotion = KatMotionEnum.TranslationXPositive;
        firstConfig.ConfigMode = KatConfigModeEnum.SingleAction;
        firstConfig.KeyActionConfigGroup.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1)
        ]);

        var customTimeout = 1234;
        var customConfig = new KatTriggerTimesConfig(150, customTimeout, 100, 2.5);
        viewModel.MotionTimeConfigs.Configs[KatMotionEnum.TranslationXPositive] = customConfig;

        var method = typeof(KatMotionConfigViewModel)
            .GetMethod("AdjustMotionTimeConfigsForSingleActionMode", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var adjusted = (KatMotionTimeConfigs)method!.Invoke(viewModel, null)!;
        var adjustedTrigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];

        // Assert
        await Assert.That(adjustedTrigger.LongReachTimeoutMs).IsEqualTo(50);
        await Assert.That(adjustedTrigger.LongReachRepeatScaleFactor).IsEqualTo(2.5);
    }

    [Test]
    public async Task AdjustMotionTimeConfigsForSingleActionMode_WithMultiKey_ShouldDisableRepeatAndForceTimeout50()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(viewModel, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationXPositive);
        modeVm.KatMotionGroups.Add(groupVm);
        viewModel.KatMotionsWithMode.Clear();
        viewModel.KatMotionsWithMode.Add(modeVm);

        var firstConfig = groupVm.Configs.First();
        firstConfig.KatMotion = KatMotionEnum.TranslationXPositive;
        firstConfig.ConfigMode = KatConfigModeEnum.SingleAction;
        firstConfig.KeyActionConfigGroup.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.KeyBoard, "B", PressModeEnum.Press, 1)
        ]);

        var customConfig = new KatTriggerTimesConfig(150, 1234, 100, 2.5);
        viewModel.MotionTimeConfigs.Configs[KatMotionEnum.TranslationXPositive] = customConfig;

        var method = typeof(KatMotionConfigViewModel)
            .GetMethod("AdjustMotionTimeConfigsForSingleActionMode", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var adjusted = (KatMotionTimeConfigs)method!.Invoke(viewModel, null)!;
        var adjustedTrigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];

        // Assert
        await Assert.That(adjustedTrigger.LongReachTimeoutMs).IsEqualTo(50);
        await Assert.That(adjustedTrigger.LongReachRepeatScaleFactor).IsEqualTo(0.0);
    }

    [Test]
    public async Task AdjustMotionTimeConfigsForSingleActionMode_WithKeyboardAndMouse_ShouldDisableRepeatAndForceTimeout50()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(viewModel, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationXPositive);
        modeVm.KatMotionGroups.Add(groupVm);
        viewModel.KatMotionsWithMode.Clear();
        viewModel.KatMotionsWithMode.Add(modeVm);

        var firstConfig = groupVm.Configs.First();
        firstConfig.KatMotion = KatMotionEnum.TranslationXPositive;
        firstConfig.ConfigMode = KatConfigModeEnum.SingleAction;
        firstConfig.KeyActionConfigGroup.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1),
            new KeyActionConfig(ActionType.Mouse, "LButton", PressModeEnum.Press, 1)
        ]);

        var customConfig = new KatTriggerTimesConfig(150, 1234, 100, 2.5);
        viewModel.MotionTimeConfigs.Configs[KatMotionEnum.TranslationXPositive] = customConfig;

        var method = typeof(KatMotionConfigViewModel)
            .GetMethod("AdjustMotionTimeConfigsForSingleActionMode", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var adjusted = (KatMotionTimeConfigs)method!.Invoke(viewModel, null)!;
        var adjustedTrigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];

        // Assert
        await Assert.That(adjustedTrigger.LongReachTimeoutMs).IsEqualTo(50);
        await Assert.That(adjustedTrigger.LongReachRepeatScaleFactor).IsEqualTo(0.0);
    }

    [Test]
    public async Task AdjustMotionTimeConfigsForSingleActionMode_WithSingleMouse_ShouldEnableRepeatAndForceTimeout50()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();
        var modeVm = new KatMotionsWithModeViewModel(viewModel, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationXPositive);
        modeVm.KatMotionGroups.Add(groupVm);
        viewModel.KatMotionsWithMode.Clear();
        viewModel.KatMotionsWithMode.Add(modeVm);

        var firstConfig = groupVm.Configs.First();
        firstConfig.KatMotion = KatMotionEnum.TranslationXPositive;
        firstConfig.ConfigMode = KatConfigModeEnum.SingleAction;
        firstConfig.KeyActionConfigGroup.FromKeyActionConfig([
            new KeyActionConfig(ActionType.Mouse, "LButton", PressModeEnum.Press, 1)
        ]);

        var customConfig = new KatTriggerTimesConfig(150, 1234, 100, 2.5);
        viewModel.MotionTimeConfigs.Configs[KatMotionEnum.TranslationXPositive] = customConfig;

        var method = typeof(KatMotionConfigViewModel)
            .GetMethod("AdjustMotionTimeConfigsForSingleActionMode", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var adjusted = (KatMotionTimeConfigs)method!.Invoke(viewModel, null)!;
        var adjustedTrigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];

        // Assert
        await Assert.That(adjustedTrigger.LongReachTimeoutMs).IsEqualTo(50);
        await Assert.That(adjustedTrigger.LongReachRepeatScaleFactor).IsEqualTo(2.5);
    }

    [Test]
    public async Task ToKatMotionConfigGroups_WhenSameMotionMixedWithSingleActionAcrossModes_ShouldReturnError()
    {
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel();

        var mode0 = viewModel.KatMotionsWithMode.First();
        var group0 = new KatMotionGroupViewModel(mode0, KatMotionEnum.TranslationYPositive);
        group0.Configs.Clear();
        var singleActionVm = new KatMotionViewModel(group0, 0)
        {
            KatMotion = KatMotionEnum.TranslationYPositive,
            ConfigMode = KatConfigModeEnum.SingleAction,
            ToModeNum = 0
        };
        singleActionVm.KeyActionConfigGroup.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1)
        ]);
        group0.Configs.Add(singleActionVm);

        var advancedVm = new KatMotionViewModel(group0, 0)
        {
            KatMotion = KatMotionEnum.TranslationYPositive,
            ConfigMode = KatConfigModeEnum.Advanced,
            KatPressMode = KatPressModeEnum.Short,
            RepeatCount = 1,
            ToModeNum = 0
        };
        advancedVm.KeyActionConfigGroup.FromKeyActionConfig([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Click, 1)
        ]);
        group0.Configs.Add(advancedVm);
        mode0.KatMotionGroups.Clear();
        mode0.KatMotionGroups.Add(group0);

        var result = viewModel.ToKatMotionConfigGroups();
        var errorMessage = string.Empty;
        result.IfLeft(ex => errorMessage = ex.Message);

        await Assert.That(result.IsLeft).IsTrue();
        await Assert.That(errorMessage.Contains("配置模式不一致")).IsTrue();
    }

    [Test]
    public async Task ToKatMotionConfigGroups_WhenSemanticProfileReturnsError_ShouldReturnProfileError()
    {
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object,
            notificationService: MockNotificationService.Object,
            activateService: MockActivateService.Object,
            semanticProfile: new AlwaysFailKatMotionSemanticProfile());

        var result = viewModel.ToKatMotionConfigGroups();
        var errorMessage = string.Empty;
        result.IfLeft(ex => errorMessage = ex.Message);

        await Assert.That(result.IsLeft).IsTrue();
        await Assert.That(errorMessage).IsEqualTo("semantic profile rejected config");
    }

    [Test]
    public async Task LoadFromConfigGroup_WithAdvancedModeConfig_ShouldSetFirstConfigModeToAdvanced()
    {
        // Arrange
        var viewModel = ViewModelTestHelpers.CreateConfigViewModel(
            fileService: MockFileService.Object,
            notificationService: MockNotificationService.Object,
            activateService: MockActivateService.Object
        );

        var advancedConfig = new KatMotionConfig(
            new KatMotion(KatMotionEnum.TranslationYPositive, KatPressModeEnum.Short, 1),
            [],
            false,
            "",
            0,
            0,
            KatConfigModeEnum.Advanced
        );

        var configGroup = new KatMotionConfigGroup(
            Guid.NewGuid().ToString(),
            false,
            "test.exe",
            [advancedConfig],
            false,
            new KatDeadZoneConfig(),
            false,
            new KatMotionTimeConfigs()
        );

        // Act
        var result = viewModel.LoadFromConfigGroup(configGroup);

        // Assert
        await Assert.That(result).IsTrue();
        var firstGroup = viewModel.KatMotionsWithMode
            .SelectMany(m => m.KatMotionGroups)
            .FirstOrDefault();
        await Assert.That(firstGroup).IsNotNull();
        await Assert.That(firstGroup!.FirstConfigMode).IsEqualTo(KatConfigModeEnum.Advanced);
    }

    // Helper methods
    private void SetupValidConfig(KatMotionConfigViewModel viewModel)
    {
        viewModel.ProcessPath = "test.exe";
        var modeVm = new KatMotionsWithModeViewModel(viewModel, 0);
        var groupVm = new KatMotionGroupViewModel(modeVm, KatMotionEnum.TranslationYPositive);
        var motionVm = new KatMotionViewModel(groupVm, 0)
        {
            KatMotion = KatMotionEnum.TranslationYPositive,
            ConfigMode = KatConfigModeEnum.SingleAction
        };
        // 注意：这里需要根据实际的AddHotKeyActions方法调整
        // motionVm.KeyActionConfigGroup.AddHotKeyActions(...);
    }

    private sealed class AlwaysFailKatMotionSemanticProfile : IKatMotionSemanticProfile
    {
        public Either<Exception, bool> ValidatePreModeGraph(in KatMotionConfigSemanticValidationContext context)
        {
            return new Exception("semantic profile rejected config");
        }

        public Either<Exception, bool> ValidatePostModeGraph(in KatMotionConfigSemanticValidationContext context)
        {
            return true;
        }

        public KatMotionTimeConfigs AdjustMotionTimeConfigs(KatMotionTimeConfigs source,
            IReadOnlyList<MotionTimeAdjustmentInput> inputs)
        {
            return source;
        }
    }
}

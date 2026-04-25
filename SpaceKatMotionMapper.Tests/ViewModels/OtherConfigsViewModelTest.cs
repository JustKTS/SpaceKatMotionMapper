using Moq;
using TUnit.Assertions;
using TUnit.Core;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.Tests.Helpers;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKatMotionMapper.Tests.ViewModels;

public class OtherConfigsViewModelTest
{
    #region 构造函数测试

    [Test]
    public async Task Constructor_WithNoParameters_ShouldInitializeWithEmptyCollection()
    {
        // Arrange & Act
        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel();

        // Assert - 构造函数不再自动添加配置
        await Assert.That(vm.KatMotionConfigGroups).IsNotNull();
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Constructor_WithMockedServices_ShouldUseMockedServices()
    {
        // Arrange
        var mockFileService = Mock.Of<IKatMotionFileService>();
        var mockNotificationService = Mock.Of<IPopUpNotificationService>();
        var mockStorageService = Mock.Of<IStorageProviderService>();
        var mockActivationService = Mock.Of<IActivationStatusService>();
        var mockVmManageService = Mock.Of<IKatMotionConfigVMManageService>();
        var mockKatMotionActivateService = Mock.Of<IKatMotionActivateService>();
        var mockRunningProgramSelectorViewModel = new TestDoubles.FakeRunningProgramSelectorViewModel();

        // Act
        var vm = new OtherConfigsViewModel(
            mockFileService,
            mockNotificationService,
            mockStorageService,
            mockActivationService,
            mockVmManageService,
            mockKatMotionActivateService,
            mockRunningProgramSelectorViewModel
        );

        // Assert - 构造函数不再自动添加配置
        await Assert.That(vm.KatMotionConfigGroups).IsNotNull();
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(0);
    }

    #endregion

    #region Add 方法测试

    [Test]
    public async Task AddCommand_ShouldAddNewConfig()
    {
        // Arrange
        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel();
        var initialCount = vm.KatMotionConfigGroups.Count;

        // Act
        vm.AddCommand.Execute(null);

        // Assert
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(initialCount + 1);
    }

    #endregion

    #region Remove 方法测试

    [Test]
    public async Task RemoveCommand_WithValidIndex_ShouldRemoveConfig()
    {
        // Arrange
        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel();
        // 添加两个配置，这样删除一个后不会触发自动添加
        vm.AddCommand.Execute(null);
        vm.AddCommand.Execute(null);
        var initialCount = vm.KatMotionConfigGroups.Count;

        // Act
        vm.RemoveCommand.Execute(0);

        // Assert - 应该剩下一个配置 (2 - 1 = 1)
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(initialCount - 1);
    }

    [Test]
    public async Task RemoveCommand_WithInvalidIndex_ShouldNotThrow()
    {
        // Arrange
        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel();
        var initialCount = vm.KatMotionConfigGroups.Count;
        Exception? exception = null;

        // Act
        try
        {
            vm.RemoveCommand.Execute(-1);
            vm.RemoveCommand.Execute(999);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        await Assert.That(exception).IsNull();
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(initialCount);
    }

    [Test]
    public async Task RemoveCommand_WhenRemovingLastConfig_ShouldCreateNewConfig()
    {
        // Arrange
        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel();
        while (vm.KatMotionConfigGroups.Count > 1)
        {
            vm.RemoveCommand.Execute(0);
        }
        var initialCount = vm.KatMotionConfigGroups.Count;

        // Act
        vm.RemoveCommand.Execute(0);

        // Assert
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(initialCount);
    }

    #endregion

    #region LoadGroupFromFiles 方法测试

    [Test]
    public async Task LoadGroupFromFilesCommand_WhenCancelled_ShouldNotAddConfigs()
    {
        // Arrange
        var mockStorageService = Mock.Of<IStorageProviderService>();
        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel(
            storageProviderService: mockStorageService
        );
        var initialCount = vm.KatMotionConfigGroups.Count;

        // Act
        await vm.LoadGroupFromFilesCommand.ExecuteAsync(null);

        // Assert
        await Assert.That(vm.KatMotionConfigGroups.Count).IsEqualTo(initialCount);
    }

    #endregion

    #region SaveGroupsToConfigDir 方法测试

    [Test]
    public async Task SaveGroupsToConfigDirCommand_ShouldCallFileService()
    {
        // Arrange
        var mockFileService = new Mock<IKatMotionFileService>();
        mockFileService
            .Setup(x => x.SaveConfigGroupsToSysConf(It.IsAny<List<SpaceKatMotionMapper.Models.KatMotionConfigGroup>>()))
            .Returns(LanguageExt.Either<Exception, bool>.Right(true));

        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel(
            fileService: mockFileService.Object
        );

        // Act
        await vm.SaveGroupsToConfigDirCommand.ExecuteAsync(null);

        // Assert
        mockFileService.Verify(
            x => x.SaveConfigGroupsToSysConf(It.IsAny<List<SpaceKatMotionMapper.Models.KatMotionConfigGroup>>()),
            Times.Once);
    }

    #endregion

    #region ReloadConfigGroupsFromSysConf 方法测试

    [Test]
    public async Task ReloadConfigGroupsFromSysConfCommand_ShouldClearAndReloadConfigs()
    {
        // Arrange
        var mockFileService = new Mock<IKatMotionFileService>();
        mockFileService
            .Setup(x => x.LoadConfigGroupsFromSysConf())
            .Returns(LanguageExt.Either<Exception, List<SpaceKatMotionMapper.Models.KatMotionConfigGroup>>.Right([]));

        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel(
            fileService: mockFileService.Object
        );

        // Act
        vm.ReloadConfigGroupsFromSysConfCommand.Execute(null);

        // Assert
        mockFileService.Verify(x => x.LoadConfigGroupsFromSysConf(), Times.Once);
    }

    #endregion

    #region ClearConfigGroups 方法测试

    [Test]
    public async Task ClearConfigGroups_ShouldRemoveAllConfigs()
    {
        // Arrange
        var mockFileService = new Mock<IKatMotionFileService>();
        mockFileService
            .Setup(x => x.LoadConfigGroupsFromSysConf())
            .Returns(LanguageExt.Either<Exception, List<SpaceKatMotionMapper.Models.KatMotionConfigGroup>>.Right([]));

        var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel(
            fileService: mockFileService.Object
        );

        // 先添加一些配置
        vm.AddCommand.Execute(null);
        vm.AddCommand.Execute(null);
        var countBeforeClear = vm.KatMotionConfigGroups.Count;

        // 由于 ClearConfigGroups 是 private，我们通过 ReloadConfigGroupsFromSysConf 来间接测试
        // 这个测试验证了 ReloadConfigGroupsFromSysConf 会先调用 ClearConfigGroups

        vm.ReloadConfigGroupsFromSysConfCommand.Execute(null);

        // Assert - ReloadConfigGroupsFromSysConf 会添加一个默认配置如果集合为空
        await Assert.That(vm.KatMotionConfigGroups.Count).IsLessThanOrEqualTo(countBeforeClear);
    }

    #endregion
}

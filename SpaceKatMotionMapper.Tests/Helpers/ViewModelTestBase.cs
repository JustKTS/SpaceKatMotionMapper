using LanguageExt;
using Moq;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKatMotionMapper.Tests.Helpers;

public abstract class ViewModelTestBase
{
    protected readonly Mock<IKatMotionFileService> MockFileService;
    protected readonly Mock<IPopUpNotificationService> MockNotificationService;
    protected readonly Mock<IKatMotionActivateService> MockActivateService;
    protected readonly Mock<IStorageProviderService> MockStorageProviderService;

    protected ViewModelTestBase()
    {
        MockFileService = new Mock<IKatMotionFileService>();
        MockNotificationService = new Mock<IPopUpNotificationService>();
        MockActivateService = new Mock<IKatMotionActivateService>();
        MockStorageProviderService = new Mock<IStorageProviderService>();

        // 设置默认行为
        SetupDefaultBehaviors();
    }

    private void SetupDefaultBehaviors()
    {
        MockFileService
            .Setup(x => x.SaveConfigGroupToSysConf(It.IsAny<KatMotionConfigGroup>()))
            .Returns(Either<Exception, bool>.Right(true));

        MockActivateService
            .Setup(x => x.ActivateKatMotions(It.IsAny<KatMotionConfigGroup>()))
            .Callback(() => { });

        // 设置 Mock 存储服务 - 简化版本
        var mockStorageProvider = new Moq.Mock<Avalonia.Platform.Storage.IStorageProvider>();
        MockStorageProviderService
            .Setup(x => x.GetStorageProvider())
            .Returns(mockStorageProvider.Object);
    }
}

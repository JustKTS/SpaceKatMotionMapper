using CSharpFunctionalExtensions;
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
        SetupDefaultBehaviors();
    }

    private void SetupDefaultBehaviors()
    {
        MockFileService
            .Setup(x => x.SaveConfigGroupToSysConf(It.IsAny<KatMotionConfigGroup>()))
            .Returns(Result.Success<bool, Exception>(true));

        MockActivateService
            .Setup(x => x.ActivateKatMotions(It.IsAny<KatMotionConfigGroup>()))
            .Callback(() => { });

        var mockStorageProvider = new Moq.Mock<Avalonia.Platform.Storage.IStorageProvider>();
        MockStorageProviderService
            .Setup(x => x.GetStorageProvider())
            .Returns(mockStorageProvider.Object);
    }
}

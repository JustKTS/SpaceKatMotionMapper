using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Tests.Services;

public class KatMotionFileServiceTests
{
    [Test]
    public void LoadConfigGroup_WhenVersion1ConfigProvided_ShouldMigrateToCurrentVersionAndPreserveActions()
    {
        var legacyConfig = CreateConfigGroup(version: 1);
        var fileService = new Mock<IFileService>();
        fileService.Setup(x => x.Read<KatMotionConfigGroup>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(legacyConfig);
        var sut = new KatMotionFileService(fileService.Object);

        Exception? error = null;
        KatMotionConfigGroup? loadedConfig = null;

        var result = sut.LoadConfigGroup(@"C:\Temp\legacy-config.json");
        if (result.IsSuccess)
            loadedConfig = result.Value;
        else
            error = result.Error;

        error.Should().BeNull();
        loadedConfig.Should().NotBeNull();
        loadedConfig!.Version.Should().Be(GlobalConstConfigs.ConfigFileVersion);
        loadedConfig.Motions.Should().HaveCount(1);
        loadedConfig.Motions[0].ActionConfigs.Should().BeEquivalentTo(legacyConfig.Motions[0].ActionConfigs,
            options => options.WithStrictOrdering());
    }

    [Test]
    public void LoadConfigGroup_WhenVersionIsUnsupported_ShouldReturnWrappedExceptionInsteadOfLooping()
    {
        var unsupportedConfig = CreateConfigGroup(version: 999);
        var fileService = new Mock<IFileService>();
        fileService.Setup(x => x.Read<KatMotionConfigGroup>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(unsupportedConfig);
        var sut = new KatMotionFileService(fileService.Object);

        Exception? error = null;
        KatMotionConfigGroup? loadedConfig = null;

        var result = sut.LoadConfigGroup(@"C:\Temp\unsupported-config.json");
        if (result.IsSuccess)
            loadedConfig = result.Value;
        else
            error = result.Error;

        loadedConfig.Should().BeNull();
        error.Should().NotBeNull();
        error!.Message.Should().Be("读取配置文件失败");
        error.InnerException.Should().NotBeNull();
        error.InnerException!.Message.Should().Contain("不支持的配置版本: 999");
    }

    [Test]
    public void KatMotionConfigGroupJson_ShouldRoundTripWithoutChangingSemanticContent()
    {
        var original = CreateConfigGroup(version: GlobalConstConfigs.ConfigFileVersion);

        var json = JsonSerializer.Serialize(original, JsonSgOption.Default);
        var roundTripped = JsonSerializer.Deserialize<KatMotionConfigGroup>(json, JsonSgOption.Default);

        roundTripped.Should().NotBeNull();
        roundTripped!.Should().BeEquivalentTo(original);
    }

    [Test]
    public void LoadConfigGroup_WhenVersion2ConfigProvided_ShouldMigrateConfigModeToAdvanced()
    {
        var legacyConfig = CreateConfigGroup(version: 2);
        var fileService = new Mock<IFileService>();
        fileService.Setup(x => x.Read<KatMotionConfigGroup>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(legacyConfig);
        var sut = new KatMotionFileService(fileService.Object);

        Exception? error = null;
        KatMotionConfigGroup? loadedConfig = null;

        var result = sut.LoadConfigGroup(@"C:\Temp\legacy-config.json");
        if (result.IsSuccess)
            loadedConfig = result.Value;
        else
            error = result.Error;

        error.Should().BeNull();
        loadedConfig.Should().NotBeNull();
        loadedConfig!.Version.Should().Be(GlobalConstConfigs.ConfigFileVersion);
        loadedConfig.Motions.Should().HaveCount(1);
        loadedConfig.Motions[0].ConfigMode.Should().Be(KatConfigModeEnum.Advanced);
    }

    private static KatMotionConfigGroup CreateConfigGroup(int version)
    {
        List<KeyActionConfig> actionConfigs =
        [
            new(ActionType.KeyBoard, KeyCodeWrapper.CONTROL.GetWrappedName(), PressModeEnum.Press, 1),
            new(ActionType.KeyBoard, KeyCodeWrapper.A.GetWrappedName(), PressModeEnum.Click, 1),
            new(ActionType.Delay, KeyActionConstants.NoneKeyValue, PressModeEnum.None, KeyActionConstants.MinDelayMultiplier)
        ];

        var motionConfig = new KatMotionConfig(
            new KatMotion(KatMotionEnum.TranslationYPositive, KatPressModeEnum.Short, 1),
            actionConfigs,
            IsCustomDescription: true,
            KeyActionsDescription: "Ctrl+A",
            ModeNum: 1,
            ToModeNum: 2);

        return new KatMotionConfigGroup(
            Guid.NewGuid().ToString(),
            IsDefault: false,
            ProcessPath: "test.exe",
            Motions: [motionConfig],
            IsCustomDeadZone: false,
            DeadZoneConfig: new KatDeadZoneConfig(),
            IsCustomMotionTimeConfigs: false,
            MotionTimeConfigs: new KatMotionTimeConfigs(),
            Version: version);
    }
}



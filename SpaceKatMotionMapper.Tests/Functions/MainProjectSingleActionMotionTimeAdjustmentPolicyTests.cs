using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Models;
using System.Collections.Generic;

namespace SpaceKatMotionMapper.Tests.Functions;

public class MainProjectSingleActionMotionTimeAdjustmentPolicyTests
{
    [Test]
    public async Task Adjust_WhenNoSingleActionInputs_ShouldReturnOriginalConfigs()
    {
        var sut = new MainProjectSingleActionMotionTimeAdjustmentPolicy();
        var source = CreateSourceConfig();

        var adjusted = sut.Adjust(source, new List<MotionTimeAdjustmentInput>
        {
            new MotionTimeAdjustmentInput(KatMotionEnum.TranslationXPositive, KatConfigModeEnum.Advanced, false)
        });

        await Assert.That(adjusted.Configs[KatMotionEnum.TranslationXPositive]).IsEqualTo(source.Configs[KatMotionEnum.TranslationXPositive]);
    }

    [Test]
    public async Task Adjust_WhenSingleActionAndSingleInput_ShouldForceTimeout50AndKeepRepeat()
    {
        var sut = new MainProjectSingleActionMotionTimeAdjustmentPolicy();
        var source = CreateSourceConfig();

        var adjusted = sut.Adjust(source, new List<MotionTimeAdjustmentInput>
        {
            new MotionTimeAdjustmentInput(KatMotionEnum.TranslationXPositive, KatConfigModeEnum.SingleAction, true)
        });

        var trigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];
        await Assert.That(trigger.LongReachTimeoutMs).IsEqualTo(50);
        await Assert.That(trigger.LongReachRepeatScaleFactor).IsEqualTo(2.5);
    }

    [Test]
    public async Task Adjust_WhenSingleActionAndMultipleInputs_ShouldDisableRepeat()
    {
        var sut = new MainProjectSingleActionMotionTimeAdjustmentPolicy();
        var source = CreateSourceConfig();

        var adjusted = sut.Adjust(source, new List<MotionTimeAdjustmentInput>
        {
            new MotionTimeAdjustmentInput(KatMotionEnum.TranslationXPositive, KatConfigModeEnum.SingleAction, true),
            new MotionTimeAdjustmentInput(KatMotionEnum.TranslationXPositive, KatConfigModeEnum.SingleAction, false)
        });

        var trigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];
        await Assert.That(trigger.LongReachTimeoutMs).IsEqualTo(50);
        await Assert.That(trigger.LongReachRepeatScaleFactor).IsEqualTo(0.0);
    }

    [Test]
    public async Task Adjust_WhenMotionIsOverridden_ShouldPassThroughUserValues()
    {
        var sut = new MainProjectSingleActionMotionTimeAdjustmentPolicy();
        var source = CreateSourceConfig();
        source.OverriddenMotions.Add(KatMotionEnum.TranslationXPositive);

        var adjusted = sut.Adjust(source, new List<MotionTimeAdjustmentInput>
        {
            new MotionTimeAdjustmentInput(KatMotionEnum.TranslationXPositive, KatConfigModeEnum.SingleAction, true)
        });

        var trigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];
        await Assert.That(trigger.LongReachTimeoutMs).IsEqualTo(1234);
        await Assert.That(trigger.LongReachRepeatScaleFactor).IsEqualTo(2.5);
    }

    [Test]
    public async Task Adjust_WhenMotionIsOverriddenAndRepeatDisabled_ShouldKeepZeroScale()
    {
        var sut = new MainProjectSingleActionMotionTimeAdjustmentPolicy();
        var source = CreateSourceConfigWithScale(0.0);
        source.OverriddenMotions.Add(KatMotionEnum.TranslationXPositive);

        var adjusted = sut.Adjust(source, new List<MotionTimeAdjustmentInput>
        {
            new MotionTimeAdjustmentInput(KatMotionEnum.TranslationXPositive, KatConfigModeEnum.SingleAction, false)
        });

        var trigger = adjusted.Configs[KatMotionEnum.TranslationXPositive];
        await Assert.That(trigger.LongReachTimeoutMs).IsEqualTo(1234);
        await Assert.That(trigger.LongReachRepeatScaleFactor).IsEqualTo(0.0);
    }

    private static KatMotionTimeConfigs CreateSourceConfig()
    {
        return new KatMotionTimeConfigs(new Dictionary<KatMotionEnum, KatTriggerTimesConfig>
        {
            [KatMotionEnum.TranslationXPositive] = new KatTriggerTimesConfig(150, 1234, 100, 2.5)
        });
    }

    private static KatMotionTimeConfigs CreateSourceConfigWithScale(double scaleFactor)
    {
        return new KatMotionTimeConfigs(new Dictionary<KatMotionEnum, KatTriggerTimesConfig>
        {
            [KatMotionEnum.TranslationXPositive] = new KatTriggerTimesConfig(150, 1234, 100, scaleFactor)
        });
    }
}

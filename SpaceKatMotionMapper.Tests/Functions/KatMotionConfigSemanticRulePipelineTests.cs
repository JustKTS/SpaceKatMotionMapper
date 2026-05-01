using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Functions.SemanticRules;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Tests.Functions;

public class KatMotionConfigSemanticRulePipelineTests
{
    [Test]
    public async Task Validate_WithNoRules_ShouldPass()
    {
        var sut = new KatMotionConfigSemanticRulePipeline(new List<IKatMotionConfigSemanticRule>());
        var context = new KatMotionConfigSemanticValidationContext(new List<KatMotionSemanticItem>());

        var result = sut.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task PressReleaseBalanceRule_ShouldRejectMissingReleaseOutsideSingleActionMode()
    {
        var sut = new PressReleaseBalanceSemanticRule();
        var context = new KatMotionConfigSemanticValidationContext(new List<KatMotionSemanticItem>
        {
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.Advanced,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1))
        });

        var result = sut.Validate(context);
        var errorMessage = string.Empty;
        if (result.IsFailure) errorMessage = result.Error.Message;

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(errorMessage.Contains("按下但没有被释放")).IsTrue();
    }

    [Test]
    public async Task PressReleaseBalanceRule_ShouldSkipCheckWhenSingleActionModeExists()
    {
        var sut = new PressReleaseBalanceSemanticRule();
        var context = new KatMotionConfigSemanticValidationContext(new List<KatMotionSemanticItem>
        {
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.SingleAction,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1))
        });

        var result = sut.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task CrossModeSingleActionConsistencyRule_ShouldRejectMixedModesForSameMotion()
    {
        var sut = new CrossModeSingleActionConsistencySemanticRule();
        var context = new KatMotionConfigSemanticValidationContext(new List<KatMotionSemanticItem>
        {
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.SingleAction,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1)),
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.Advanced,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Click, 1))
        });

        var result = sut.Validate(context);
        var errorMessage = string.Empty;
        if (result.IsFailure) errorMessage = result.Error.Message;

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(errorMessage.Contains("配置模式不一致")).IsTrue();
    }

    [Test]
    public async Task MainProjectAssembler_ShouldProvidePreAndPostRulePipelines()
    {
        var assembler = new MainProjectKatMotionSemanticRuleAssembler();

        var preResult = MainProjectKatMotionSemanticRuleAssembler.CreatePreModeGraphValidator().Validate(new KatMotionConfigSemanticValidationContext(new List<KatMotionSemanticItem>
        {
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.Advanced,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1))
        }));
        var postResult = MainProjectKatMotionSemanticRuleAssembler.CreatePostModeGraphValidator().Validate(new KatMotionConfigSemanticValidationContext(new List<KatMotionSemanticItem>
        {
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.SingleAction,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1)),
            CreateItem(KatMotionEnum.TranslationYPositive, KatConfigModeEnum.Advanced,
                new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Click, 1))
        }));

        await Assert.That(preResult.IsFailure).IsTrue();
        await Assert.That(postResult.IsFailure).IsTrue();
    }

    private static KatMotionSemanticItem CreateItem(
        KatMotionEnum motion,
        KatConfigModeEnum configMode,
        params KeyActionConfig[] actions)
    {
        return new KatMotionSemanticItem(motion, configMode, actions.ToList());
    }
}




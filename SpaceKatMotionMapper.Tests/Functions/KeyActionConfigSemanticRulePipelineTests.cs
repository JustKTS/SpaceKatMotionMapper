using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Functions.Contract;
using SpaceKatMotionMapper.Functions.SemanticRules;

namespace SpaceKatMotionMapper.Tests.Functions;

public class KeyActionConfigSemanticRulePipelineTests
{
    [Test]
    public async Task Validate_WithNoRules_ShouldReturnTrue()
    {
        var sut = new KeyActionConfigSemanticRulePipeline([]);

        var isValid = sut.Validate([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Click, 1)
        ], isSingleActionMode: true);

        await Assert.That(isValid).IsTrue();
    }

    [Test]
    public async Task Validate_WithSingleActionPressOnlyRule_ShouldRejectClickInSingleActionMode()
    {
        var sut = new KeyActionConfigSemanticRulePipeline([new SingleActionPressOnlySemanticRule()]);

        var isValid = sut.Validate([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Click, 1)
        ], isSingleActionMode: true);

        await Assert.That(isValid).IsFalse();
    }

    [Test]
    public async Task Validate_WithSingleActionPressOnlyRule_ShouldPassPressInSingleActionMode()
    {
        var sut = new KeyActionConfigSemanticRulePipeline([new SingleActionPressOnlySemanticRule()]);

        var isValid = sut.Validate([
            new KeyActionConfig(ActionType.KeyBoard, "A", PressModeEnum.Press, 1)
        ], isSingleActionMode: true);

        await Assert.That(isValid).IsTrue();
    }

    [Test]
    public async Task Validate_ShouldStopWhenAnyRuleFails()
    {
        var sut = new KeyActionConfigSemanticRulePipeline([
            new AlwaysTrueRule(),
            new AlwaysFalseRule(),
            new AlwaysTrueRule()
        ]);

        var isValid = sut.Validate([], isSingleActionMode: false);

        await Assert.That(isValid).IsFalse();
    }

    private sealed class AlwaysTrueRule : IKeyActionConfigSemanticRule
    {
        public bool Validate(in KeyActionSemanticValidationContext context)
        {
            return true;
        }
    }

    private sealed class AlwaysFalseRule : IKeyActionConfigSemanticRule
    {
        public bool Validate(in KeyActionSemanticValidationContext context)
        {
            return false;
        }
    }
}


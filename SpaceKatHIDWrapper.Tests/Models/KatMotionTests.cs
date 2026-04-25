using SpaceKatHIDWrapper.Models;
using TUnit.Assertions;
using TUnit.Core;

namespace SpaceKatHIDWrapper.Tests.Models;

public class KatMotionTests
{
    // === Short mode: RepeatCount matters ===

    [Test]
    public async Task MatchesMotion_Short_SameRepeatCount_ShouldMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);

        await Assert.That(a.MatchesMotion(b)).IsTrue();
    }

    [Test]
    public async Task MatchesMotion_Short_DifferentRepeatCount_ShouldNotMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 2);

        await Assert.That(a.MatchesMotion(b)).IsFalse();
    }

    // === LongReach mode: RepeatCount ignored ===

    [Test]
    public async Task MatchesMotion_LongReach_DifferentRepeatCount_ShouldStillMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 5);

        await Assert.That(a.MatchesMotion(b)).IsTrue();
    }

    [Test]
    public async Task MatchesMotion_LongReach_SameRepeatCount_ShouldMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 3);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 3);

        await Assert.That(a.MatchesMotion(b)).IsTrue();
    }

    // === LongDown mode: RepeatCount ignored (long press doesn't support RepeatCount) ===

    [Test]
    public async Task MatchesMotion_LongDown_SameRepeatCount_ShouldMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 1);

        await Assert.That(a.MatchesMotion(b)).IsTrue();
    }

    [Test]
    public async Task MatchesMotion_LongDown_DifferentRepeatCount_ShouldStillMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 2);

        await Assert.That(a.MatchesMotion(b)).IsTrue();
    }

    // === Different Motion type: never matches ===

    [Test]
    public async Task MatchesMotion_DifferentMotionType_ShouldNotMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXNegative, KatPressModeEnum.Short, 1);

        await Assert.That(a.MatchesMotion(b)).IsFalse();
    }

    // === Different PressMode: never matches ===

    [Test]
    public async Task MatchesMotion_DifferentPressMode_ShouldNotMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 1);

        await Assert.That(a.MatchesMotion(b)).IsFalse();
    }

    // === Cross-mode: LongReach vs Short with same RepeatCount ===

    [Test]
    public async Task MatchesMotion_LongReachVsShort_SameRepeatCount_ShouldNotMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);

        await Assert.That(a.MatchesMotion(b)).IsFalse();
    }

    // === Null mode ===

    [Test]
    public async Task MatchesMotion_NullMode_SameMotionAndRepeat_ShouldMatch()
    {
        var a = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Null, 1);
        var b = new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Null, 1);

        await Assert.That(a.MatchesMotion(b)).IsTrue();
    }
}

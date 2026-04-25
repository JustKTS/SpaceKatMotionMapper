using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services;
using TUnit.Assertions;
using TUnit.Core;

namespace SpaceKatMotionMapper.Tests.Services;

public class ConflictKatMotionServiceTests
{
    private ConflictKatMotionService CreateService()
    {
        return new ConflictKatMotionService();
    }

    // === Short mode conflicts ===

    [Test]
    public async Task IsConflict_Short_SameMotionSameRepeatCount_ShouldBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)));

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsConflict_Short_SameMotionDifferentRepeatCount_ShouldNotBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)));

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 2);

        await Assert.That(result).IsFalse();
    }

    // === LongReach mode conflicts: RepeatCount should be ignored ===

    [Test]
    public async Task IsConflict_LongReach_SameMotionDifferentRepeatCount_ShouldBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 1)));

        // This is the bug: RepeatCount=5 vs registered RepeatCount=1
        // After fix, LongReach should ignore RepeatCount → should be conflict
        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 5);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsConflict_LongReach_SameMotionSameRepeatCount_ShouldBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 3)));

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongReach, 3);

        await Assert.That(result).IsTrue();
    }

    // === LongDown mode conflicts: RepeatCount matters ===

    [Test]
    public async Task IsConflict_LongDown_SameMotionSameRepeatCount_ShouldBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 1)));

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 1);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsConflict_LongDown_SameMotionDifferentRepeatCount_ShouldBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 1)));

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.LongDown, 2);

        await Assert.That(result).IsTrue();
    }

    // === Cross-config: different config ID should not conflict ===

    [Test]
    public async Task IsConflict_DifferentConfigId_ShouldNotBeConflict()
    {
        var service = CreateService();
        var configId1 = Guid.NewGuid();
        var configId2 = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId1,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)));

        var result = service.IsConflict(configId2,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);

        await Assert.That(result).IsFalse();
    }

    // === Different motion type: no conflict ===

    [Test]
    public async Task IsConflict_DifferentMotionType_ShouldNotBeConflict()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)));

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXNegative, KatPressModeEnum.Short, 1);

        await Assert.That(result).IsFalse();
    }

    // === RemoveByGuid ===

    [Test]
    public async Task RemoveByGuid_ShouldRemoveConflicts()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)));

        service.RemoveByGuid(configId);

        var result = service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1);

        await Assert.That(result).IsFalse();
    }

    // === Multiple registrations for same config ===

    [Test]
    public async Task IsConflict_MultipleRegisteredMotions_ShouldMatchAny()
    {
        var service = CreateService();
        var configId = Guid.NewGuid();
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)));
        service.Register(new KatMotionInfo(configId,
            new KatMotion(KatMotionEnum.TranslationYPositive, KatPressModeEnum.LongReach, 3)));

        await Assert.That(service.IsConflict(configId,
            KatMotionEnum.TranslationXPositive, KatPressModeEnum.Short, 1)).IsTrue();

        // LongReach with different RepeatCount should still conflict after fix
        await Assert.That(service.IsConflict(configId,
            KatMotionEnum.TranslationYPositive, KatPressModeEnum.LongReach, 7)).IsTrue();
    }
}
using TUnit.Assertions;
using TUnit.Core;
using SpaceKatMotionMapper.Functions;

namespace SpaceKatMotionMapper.Tests.Functions;

public class ModeChangeValidatorTests
{
    // 测试辅助方法
    private static ModeChangeValidator CreateValidator()
    {
        return new ModeChangeValidator();
    }

    private static void AddModes(ModeChangeValidator validator, params int[] modes)
    {
        foreach (var mode in modes)
        {
            validator.AddNode(mode);
        }
    }

    private static void AddTransitions(ModeChangeValidator validator, (int from, int to)[] transitions)
    {
        foreach (var (from, to) in transitions)
        {
            validator.AddEdge(from, to);
        }
    }

    [Test]
    public async Task Validate_SingleMode_ShouldReturnEmptyLists()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        await Assert.That(cannotTo).IsEmpty();
        await Assert.That(cannotReturn).IsEmpty();
    }

    [Test]
    public async Task Validate_ConnectedGraph_ShouldReturnEmptyLists()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2);
        AddTransitions(validator, [(0, 1), (1, 2), (2, 0)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        await Assert.That(cannotTo).IsEmpty();
        await Assert.That(cannotReturn).IsEmpty();
    }

    [Test]
    public async Task Validate_UnreachableNode_ShouldReturnInCannotToList()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2);
        AddTransitions(validator, [(0, 1)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        // 从0可达：0、1，无法到达：2
        // 能返回0的：0（1无法返回）
        await Assert.That(cannotTo).IsEquivalentTo([2]);
        await Assert.That(cannotReturn).IsEquivalentTo([1]);
    }

    [Test]
    public async Task Validate_CannotReturnNode_ShouldReturnInCannotReturnList()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2);
        AddTransitions(validator, [(0, 1), (0, 2)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        await Assert.That(cannotTo).IsEmpty();
        await Assert.That(cannotReturn).IsEquivalentTo([1, 2]);
    }

    [Test]
    public async Task Validate_ComplexScenario_ShouldIdentifyBothTypes()
    {
        // Arrange
        // 模式0 -> 模式1 -> 模式2
        // 模式0 -> 模式3 -> 模式4
        // 从0可达：0、1、2、3、4
        // 能返回0的：0（其他都无法返回）
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2, 3, 4);
        AddTransitions(validator, [(0, 1), (1, 2), (0, 3), (3, 4)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        await Assert.That(cannotTo).IsEmpty();
        await Assert.That(cannotReturn.Count).IsEqualTo(4);
        await Assert.That(cannotReturn).Contains(1);
        await Assert.That(cannotReturn).Contains(2);
        await Assert.That(cannotReturn).Contains(3);
        await Assert.That(cannotReturn).Contains(4);
    }

    [Test]
    public async Task Validate_NonSequentialModeNumbers_ShouldWorkCorrectly()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0, 5, 10);
        AddTransitions(validator, [(0, 5)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        // 从0可达：0、5，无法到达：10
        // 能返回0的：0（5无法返回）
        await Assert.That(cannotTo).IsEquivalentTo([10]);
        await Assert.That(cannotReturn).IsEquivalentTo([5]);
    }

    [Test]
    public async Task Validate_DuplicateEdges_ShouldNotDuplicateInAdjacencyList()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2);
        // 添加重复的边
        validator.AddEdge(0, 1);
        validator.AddEdge(0, 1);
        validator.AddEdge(0, 1);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        // 即使添加重复边，验证逻辑仍应正常工作
        await Assert.That(cannotTo).IsEquivalentTo([2]);
        await Assert.That(cannotReturn).IsEquivalentTo([1]);
    }

    [Test]
    public async Task Validate_MultipleUnreachableNodes_ShouldReturnAllInCannotToList()
    {
        // Arrange
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2, 3, 4, 5);
        AddTransitions(validator, [(0, 1), (1, 2)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        // 从0可达：0、1、2
        // 无法到达：3、4、5
        await Assert.That(cannotTo.Count).IsEqualTo(3);
        await Assert.That(cannotTo).Contains(3);
        await Assert.That(cannotTo).Contains(4);
        await Assert.That(cannotTo).Contains(5);
    }

    [Test]
    public async Task Validate_CycleNotIncludingStart_ShouldReturnInCannotReturnList()
    {
        // Arrange
        // 0 -> 1 -> 2 -> 1 (循环，但不包含0)
        var validator = CreateValidator();
        AddModes(validator, 0, 1, 2);
        AddTransitions(validator, [(0, 1), (1, 2), (2, 1)]);

        // Act
        var (cannotTo, cannotReturn) = validator.Validate();

        // Assert
        await Assert.That(cannotTo).IsEmpty();
        await Assert.That(cannotReturn).IsEquivalentTo([1, 2]);
    }
}

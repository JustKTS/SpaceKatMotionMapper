using TUnit.Assertions;
using TUnit.Core;
using SpaceKatMotionMapper.Tests.Helpers;

namespace SpaceKatMotionMapper.Tests;

public class SimpleTest
{
    [Test]
    public async Task Test_CreateViewModel_ShouldNotThrow()
    {
        // Arrange
        Exception? exception = null;

        try
        {
            // Act
            var vm = ViewModelTestHelpers.CreateConfigViewModel();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        await Assert.That(exception).IsNull();
    }

    [Test]
    public async Task Test_CreateOtherConfigsViewModel_ShouldNotThrow()
    {
        // Arrange
        Exception? exception = null;

        try
        {
            // Act
            var vm = ViewModelTestHelpers.CreateOtherConfigsViewModel();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        await Assert.That(exception).IsNull();
    }
}


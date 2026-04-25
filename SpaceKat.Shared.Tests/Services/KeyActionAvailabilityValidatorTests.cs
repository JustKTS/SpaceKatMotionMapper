using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Functions.Contract;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Tests.Services;

public class KeyActionAvailabilityValidatorTests
{
    [Test]
    public async Task Validate_Delay_ShouldUseMinDelayBoundary()
    {
        var sut = new KeyActionAvailabilityValidator();

        var invalid = sut.Validate(ActionType.Delay, KeyActionConstants.NoneKeyValue, PressModeEnum.None,
            KeyActionConstants.MinDelayMultiplier - 1, new KeyActionAvailabilityValidationOptions(true));
        var valid = sut.Validate(ActionType.Delay, KeyActionConstants.NoneKeyValue, PressModeEnum.None,
            KeyActionConstants.MinDelayMultiplier, new KeyActionAvailabilityValidationOptions(true));

        await Assert.That(invalid).IsFalse();
        await Assert.That(valid).IsTrue();
    }

    [Test]
    public async Task Validate_ScrollMouse_ShouldRespectOption()
    {
        var sut = new KeyActionAvailabilityValidator();

        var strictPositive = sut.Validate(ActionType.Mouse, nameof(MouseButtonEnum.ScrollUp), PressModeEnum.None, -1,
            new KeyActionAvailabilityValidationOptions(true));
        var nonZero = sut.Validate(ActionType.Mouse, nameof(MouseButtonEnum.ScrollUp), PressModeEnum.None, -1,
            new KeyActionAvailabilityValidationOptions(false));

        await Assert.That(strictPositive).IsFalse();
        await Assert.That(nonZero).IsTrue();
    }
}


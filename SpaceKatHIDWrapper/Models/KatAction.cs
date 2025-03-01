namespace SpaceKatHIDWrapper.Models;

public record KatAction(KatMotionEnum Motion, KatPressModeEnum KatPressMode, int RepeatCount)
{
    public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.Now;
};
namespace SpaceKatHIDWrapper.Models;

public record KatTriggerTimesConfig(
    int ShortRepeatToleranceMs,
    int LongReachTimeoutMs,
    int LongReachRepeatTimeSpanMs,
    double LongReachRepeatScaleFactor)
{
    public KatTriggerTimesConfig() : this(200, 800, 100, 1.5)
    {
    }
}
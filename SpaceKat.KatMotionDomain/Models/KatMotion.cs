using System.Text.Json.Serialization;

namespace SpaceKatHIDWrapper.Models;

public record KatMotionWithTimeStamp(KatMotionEnum Motion, KatPressModeEnum KatPressMode, int RepeatCount)
{
    public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.Now;

    public KatMotion ToKatMotion()
    {
        return new KatMotion(Motion, KatPressMode, RepeatCount);
    }
}

public record KatMotion(KatMotionEnum Motion, KatPressModeEnum KatPressMode, int RepeatCount)
{
    public bool MatchesMotion(KatMotion other)
    {
        if (Motion != other.Motion) return false;
        if (KatPressMode != other.KatPressMode) return false;
        if (KatPressMode == KatPressModeEnum.Short && RepeatCount != other.RepeatCount) return false;
        return true;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatMotion))]
[JsonSerializable(typeof(KatMotionWithTimeStamp))]
[JsonSerializable(typeof(KatMotionEnum))]
[JsonSerializable(typeof(KatPressModeEnum))]
[JsonSerializable(typeof(int))]
public partial class KatMotionJsonSgContext : JsonSerializerContext
{
}
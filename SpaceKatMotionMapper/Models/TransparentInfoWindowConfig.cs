using System.Text.Json.Serialization;

namespace SpaceKatMotionMapper.Models;

public record TransparentInfoWindowConfig(
    int X,
    int Y,
    double Width,
    double Height,
    uint BackgroundColor,
    uint FontColor,
    double FontSize = 15,
    int DisappearTimeMs = 1500,
    int AnimationTimeMs = 250);

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(TransparentInfoWindowConfig))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(uint))]
internal partial class TransparentInfoWindowConfigJsonSgContext : JsonSerializerContext
{
}
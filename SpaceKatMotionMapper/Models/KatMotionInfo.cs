using System;
using System.Text.Json.Serialization;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatMotionInfo(
    Guid Id,
    KatMotion Motion);
    
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatMotionInfo))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(KatMotion))]
internal partial class KatMotionInfoJsonSgContext : JsonSerializerContext
{
}
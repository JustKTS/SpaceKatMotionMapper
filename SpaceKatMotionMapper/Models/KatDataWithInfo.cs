using System;
using System.Text.Json.Serialization;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatDataWithInfo(bool ConfigIsDefault, Guid ActivatedConfigId, int Mode, KatMotion KatMotion);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatDataWithInfo))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(KatMotion))]
internal partial class KatDataWithInfoJsonSgContext : JsonSerializerContext
{
}
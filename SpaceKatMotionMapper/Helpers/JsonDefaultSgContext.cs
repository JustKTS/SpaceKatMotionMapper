using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Helpers;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotKeyRecord))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(Dictionary<Guid, bool>))]
[JsonSerializable(typeof(Dictionary<KatMotionEnum, KatTriggerTimesConfig>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(JsonElement))]
internal partial class JsonDefaultSgContext : JsonSerializerContext
{
}
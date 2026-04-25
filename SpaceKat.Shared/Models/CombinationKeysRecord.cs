using System.Text.Json.Serialization;

namespace SpaceKat.Shared.Models;

public record CombinationKeysRecord(bool UseCtrl, bool UseShift, bool UseAlt, bool UseWin, KeyCodeWrapper Key);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CombinationKeysRecord))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(KeyCodeWrapper))]
public partial class CombinationKeysRecordJsonSgContext : JsonSerializerContext
{
}
using System.Text.Json.Serialization;
using WindowsInput;

namespace SpaceKat.Shared.Models;

public record CombinationKeysRecord(bool UseCtrl, bool UseShift, bool UseAlt, bool UseWin, VirtualKeyCode Key);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CombinationKeysRecord))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(VirtualKeyCode))]
public partial class CombinationKeysRecordJsonSgContext : JsonSerializerContext
{
}
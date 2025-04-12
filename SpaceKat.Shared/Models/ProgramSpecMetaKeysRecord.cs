using System.Text.Json.Serialization;

namespace SpaceKat.Shared.Models;

public record ProgramSpecMetaKeysRecord(
    string ConfigName,
    string Contributors,
    bool IsGeneral,
    Dictionary<string, CombinationKeysRecord> CombinationKeys,
    Dictionary<string, List<KeyActionConfig>> MacroKeys);

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ProgramSpecMetaKeysRecord))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(CombinationKeysRecord))]
[JsonSerializable(typeof(KeyActionConfig))]
[JsonSerializable(typeof(KeyActionConfig))]
[JsonSerializable(typeof(List<KeyActionConfig>))]
[JsonSerializable(typeof(Dictionary<string, CombinationKeysRecord>))]
[JsonSerializable(typeof(Dictionary<string, List<KeyActionConfig>>))]
public partial class ProgramSpecMetaKeysRecordJsonSgContext : JsonSerializerContext
{
}
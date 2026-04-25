using System.Text.Json.Serialization;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Models;

public record HotKeyRecord(
    bool UseCtrl,
    bool UseAlt,
    bool UseShift,
    KeyCodeWrapper HotKey,
    KatButtonEnum BindKatButtonEnum);
    
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotKeyRecord))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(KatButtonEnum))]
internal partial class HotKeyRecordJsonSgContext : JsonSerializerContext
{
}
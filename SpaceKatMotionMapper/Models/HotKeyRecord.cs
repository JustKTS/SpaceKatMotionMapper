using System.Text.Json.Serialization;

namespace SpaceKatMotionMapper.Models;

public record HotKeyRecord(
    bool UseCtrl,
    bool UseAlt,
    bool UseShift,
    HotKeyCodeEnum HotKey,
    KatButtonEnum BindKatButtonEnum);
    
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotKeyRecord))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(HotKeyCodeEnum))]
[JsonSerializable(typeof(KatButtonEnum))]
internal partial class HotKeyRecordJsonSgContext : JsonSerializerContext
{
}
using System.Text.Json.Serialization;
using WindowsInput;

namespace SpaceKatMotionMapper.Models;

public record HotKeyRecord(
    bool UseCtrl,
    bool UseAlt,
    bool UseShift,
    VirtualKeyCode HotKey,
    KatButtonEnum BindKatButtonEnum);
    
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotKeyRecord))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(KatButtonEnum))]
internal partial class HotKeyRecordJsonSgContext : JsonSerializerContext
{
}
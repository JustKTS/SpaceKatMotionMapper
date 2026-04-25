using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinuxHelpers.Models.Niri;

/// <summary>
/// Niri JSON 序列化上下文
/// 使用源生成器提供高性能的 JSON 序列化/反序列化
/// </summary>
[JsonSerializable(typeof(NiriFocusedWindowResponse))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
public partial class NiriJsonSgContext : JsonSerializerContext
{
}

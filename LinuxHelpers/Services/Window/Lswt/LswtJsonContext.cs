using System.Text.Json.Serialization;
using LinuxHelpers.Services.Window.Lswt;

namespace LinuxHelpers.Services.Window.Lswt;

/// <summary>
/// lswt JSON 序列化上下文
/// 使用 Source Generator 生成高性能的序列化代码
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(LswtResponse))]
[JsonSerializable(typeof(LswtToplevel))]
[JsonSerializable(typeof(List<LswtToplevel>))]
public partial class LswtJsonContext : JsonSerializerContext
{
}

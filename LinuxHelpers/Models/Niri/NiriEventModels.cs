using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinuxHelpers.Models.Niri;

/// <summary>
/// Niri 事件流的基本事件模型
/// 使用 JsonExtensionData 处理动态事件类型
/// </summary>
public class NiriEventStreamResponse
{
    /// <summary>
    /// 扩展数据，用于处理不同类型的事件
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Events { get; set; }
}

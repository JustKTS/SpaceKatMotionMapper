using System.Text.Json.Serialization;

namespace LinuxHelpers.Services.Window.Lswt;

/// <summary>
/// lswt -j 命令的响应根对象
/// </summary>
public class LswtResponse
{
    [JsonPropertyName("json-output-version")]
    public int JsonOutputVersion { get; set; }

    [JsonPropertyName("supported-data")]
    public LswtSupportedData? SupportedData { get; set; }

    [JsonPropertyName("toplevels")]
    public List<LswtToplevel> Toplevels { get; set; } = new();
}

/// <summary>
/// lswt 支持的数据类型标识
/// </summary>
public class LswtSupportedData
{
    [JsonPropertyName("title")]
    public bool Title { get; set; }

    [JsonPropertyName("app-id")]
    public bool AppId { get; set; }

    [JsonPropertyName("identifier")]
    public bool Identifier { get; set; }

    [JsonPropertyName("fullscreen")]
    public bool Fullscreen { get; set; }

    [JsonPropertyName("activated")]
    public bool Activated { get; set; }

    [JsonPropertyName("minimized")]
    public bool Minimized { get; set; }

    [JsonPropertyName("maximized")]
    public bool Maximized { get; set; }
}

/// <summary>
/// lswt 的窗口（toplevel）信息
/// </summary>
public class LswtToplevel
{
    [JsonPropertyName("activated")]
    public bool Activated { get; set; }

    [JsonPropertyName("fullscreen")]
    public bool Fullscreen { get; set; }

    [JsonPropertyName("minimized")]
    public bool Minimized { get; set; }

    [JsonPropertyName("maximized")]
    public bool Maximized { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("app-id")]
    public string? AppId { get; set; }
}

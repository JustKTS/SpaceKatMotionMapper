using System.Text.Json.Serialization;

namespace LinuxHelpers.Models.Niri;

/// <summary>
/// Niri 窗口管理器 focused-window 命令的响应模型
/// </summary>
public class NiriFocusedWindowResponse
{
    /// <summary>
    /// 窗口唯一标识符
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 窗口标题
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 应用程序 ID (app-id)
    /// </summary>
    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }

    /// <summary>
    /// 进程 ID
    /// </summary>
    [JsonPropertyName("pid")]
    public int Pid { get; set; }

    /// <summary>
    /// 工作区 ID
    /// </summary>
    [JsonPropertyName("workspace_id")]
    public long WorkspaceId { get; set; }

    /// <summary>
    /// 是否为焦点窗口
    /// </summary>
    [JsonPropertyName("is_focused")]
    public bool IsFocused { get; set; }
}

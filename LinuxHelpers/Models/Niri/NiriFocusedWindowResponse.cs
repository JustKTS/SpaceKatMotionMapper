using System.Text.Json.Serialization;

namespace LinuxHelpers.Models.Niri;

public class NiriFocusedWindowResponse
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }
}

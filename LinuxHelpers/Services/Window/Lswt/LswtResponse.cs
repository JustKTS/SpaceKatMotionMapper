using System.Text.Json.Serialization;

namespace LinuxHelpers.Services.Window.Lswt;

public class LswtResponse
{
    [JsonPropertyName("toplevels")]
    public List<LswtToplevel> Toplevels { get; set; } = new();
}

public class LswtToplevel
{
    [JsonPropertyName("minimized")]
    public bool Minimized { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("app-id")]
    public string? AppId { get; set; }
}

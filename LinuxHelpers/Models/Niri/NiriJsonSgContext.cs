using System.Text.Json.Serialization;

namespace LinuxHelpers.Models.Niri;

[JsonSerializable(typeof(NiriFocusedWindowResponse))]
public partial class NiriJsonSgContext : JsonSerializerContext
{
}

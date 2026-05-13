using System.Text.Json.Serialization;
using LinuxHelpers.Services.Window.Lswt;

namespace LinuxHelpers.Services.Window.Lswt;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(LswtResponse))]
public partial class LswtJsonContext : JsonSerializerContext
{
}

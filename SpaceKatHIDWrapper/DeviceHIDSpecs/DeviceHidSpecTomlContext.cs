using Tomlyn.Model;
using Tomlyn.Serialization;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

[TomlSerializable(typeof(TomlTable))]
internal partial class DeviceHidSpecTomlContext : TomlSerializerContext
{
}

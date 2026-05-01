namespace SpaceKatHIDWrapper.Tests.DeviceHIDSpecs;

using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SpaceKatHIDWrapper.DeviceHIDSpecs;
using SpaceKatHIDWrapper.Models;
using TUnit.Assertions;
using TUnit.Core;

public class DeviceHidSpecTomlTests
{
    [Test]
    public async Task RoundTrip_WriterThenReader_ProducesEquivalentSpec()
    {
        var axes = new Dictionary<MotionAxis, AxisSpecs>
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(2, 1, 2, -1),
            [MotionAxis.Roll] = new(2, 3, 4, -1),
            [MotionAxis.Yaw] = new(2, 5, 6, 1),
        }.ToFrozenDictionary();

        var buttons = new List<ButtonSpecs>
        {
            new(3, 1, 0),
            new(3, 1, 1),
            new(3, 2, 4),
            new(3, 2, 5),
        }.AsReadOnly();

        var original = new Dictionary<string, DeviceHidSpec>
        {
            ["TestDevice"] = new("Test Device", [0x256F, 0xC635], axes, buttons, 350.0),
            ["TestDevice2"] = new("Test Device 2", [0x46D, 0xC627], axes,
                new List<ButtonSpecs>().AsReadOnly(), 400.0),
        };

        var toml = DeviceHidSpecTomlWriter.WriteToml(original);
        var parsed = DeviceHidSpecTomlReader.Parse(toml);

        await Assert.That(parsed.Count).IsEqualTo(2);
        await Assert.That(parsed.ContainsKey("TestDevice")).IsTrue();
        await Assert.That(parsed.ContainsKey("TestDevice2")).IsTrue();

        var d1 = parsed["TestDevice"];
        await Assert.That(d1.HidId[0]).IsEqualTo(0x256F);
        await Assert.That(d1.HidId[1]).IsEqualTo(0xC635);
        await Assert.That(d1.AxisScale).IsEqualTo(350.0);
        await Assert.That(d1.ButtonMappings.Count).IsEqualTo(4);
        await Assert.That(d1.AxesMappings[MotionAxis.X]).IsEqualTo(new AxisSpecs(1, 1, 2, 1));
        await Assert.That(d1.AxesMappings[MotionAxis.Y]).IsEqualTo(new AxisSpecs(1, 3, 4, -1));

        var d2 = parsed["TestDevice2"];
        await Assert.That(d2.ButtonMappings.Count).IsEqualTo(0);
        await Assert.That(d2.AxisScale).IsEqualTo(400.0);
    }

    [Test]
    public async Task Parse_HandlesMissingOptionalFields()
    {
        var toml = "[NoButtonsDev]\nhid_id = [0x256F, 0xC641]\naxis_scale = 350.0\n"
                 + "mappings.x = [1, 1, 2, 1]\nmappings.y = [1, 3, 4, -1]\nmappings.z = [1, 5, 6, -1]\n"
                 + "mappings.pitch = [2, 1, 2, -1]\nmappings.roll = [2, 3, 4, -1]\nmappings.yaw = [2, 5, 6, 1]\n";

        var parsed = DeviceHidSpecTomlReader.Parse(toml);

        await Assert.That(parsed.Count).IsEqualTo(1);
        var d = parsed["NoButtonsDev"];
        await Assert.That(d.ButtonMappings.Count).IsEqualTo(0);
        await Assert.That(d.AxesMappings.Count).IsEqualTo(6);
    }

    [Test]
    public async Task Parse_SkipsUnknownKeys()
    {
        var toml = "[Device]\nhid_id = [0x256F, 0xC635]\naxis_scale = 350.0\n"
                 + "led_id = [0x04, 0x01]\n"
                 + "mappings.x = [1, 1, 2, 1]\nmappings.y = [1, 3, 4, -1]\nmappings.z = [1, 5, 6, -1]\n"
                 + "mappings.pitch = [2, 1, 2, -1]\nmappings.roll = [2, 3, 4, -1]\nmappings.yaw = [2, 5, 6, 1]\n";

        var parsed = DeviceHidSpecTomlReader.Parse(toml);

        await Assert.That(parsed.Count).IsEqualTo(1);
        await Assert.That(parsed["Device"].AxesMappings.Count).IsEqualTo(6);
    }
}

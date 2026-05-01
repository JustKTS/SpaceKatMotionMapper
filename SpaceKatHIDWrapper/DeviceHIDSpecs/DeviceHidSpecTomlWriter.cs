using System.Text;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

public static class DeviceHidSpecTomlWriter
{
    private static readonly MotionAxis[] AxisOrder =
    [
        MotionAxis.X, MotionAxis.Y, MotionAxis.Z,
        MotionAxis.Pitch, MotionAxis.Roll, MotionAxis.Yaw
    ];

    public static string WriteDevice(KeyValuePair<string, DeviceHidSpec> kvp)
    {
        var sb = new StringBuilder();
        var spec = kvp.Value;

        sb.AppendLine($"[{kvp.Key}]");
        sb.AppendLine($"hid_id = [0x{spec.HidId[0]:X4}, 0x{spec.HidId[1]:X4}]");
        sb.AppendLine($"axis_scale = {spec.AxisScale:F1}");

        foreach (var axis in AxisOrder)
        {
            if (spec.AxesMappings.TryGetValue(axis, out var am))
            {
                var axisName = axis.ToString().ToLowerInvariant();
                var flipStr = am.Flip % 1 == 0 ? ((int)am.Flip).ToString() : am.Flip.ToString();
                sb.AppendLine($"mappings.{axisName,-5} = [{am.Channel}, {am.ByteIndex1}, {am.ByteIndex2}, {flipStr}]");
            }
        }

        if (spec.ButtonMappings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"[{kvp.Key}.buttons]");
            for (var i = 0; i < spec.ButtonMappings.Count; i++)
            {
                var b = spec.ButtonMappings[i];
                sb.AppendLine($"BTN_{i} = [{b.Channel}, {b.ByteIndex}, {b.Bit}]");
            }
        }

        return sb.ToString();
    }

    public static string WriteToml(IEnumerable<KeyValuePair<string, DeviceHidSpec>> devices, string? header = null)
    {
        var sb = new StringBuilder();
        if (header != null)
        {
            sb.Append(header);
            sb.AppendLine();
        }

        var first = true;
        foreach (var device in devices)
        {
            if (!first) sb.AppendLine();
            sb.Append(WriteDevice(device));
            first = false;
        }

        return sb.ToString();
    }
}

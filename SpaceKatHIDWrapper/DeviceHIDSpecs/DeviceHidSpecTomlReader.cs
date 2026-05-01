using System.Collections.Frozen;
using System.Collections.ObjectModel;
using Tomlyn;
using Tomlyn.Model;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

public static class DeviceHidSpecTomlReader
{
    private static readonly (string Name, MotionAxis Axis)[] AxisKeys =
    [
        ("x", MotionAxis.X), ("y", MotionAxis.Y), ("z", MotionAxis.Z),
        ("pitch", MotionAxis.Pitch), ("roll", MotionAxis.Roll), ("yaw", MotionAxis.Yaw)
    ];

    public static FrozenDictionary<string, DeviceHidSpec> ReadFromFile(string filePath)
    {
        var text = File.ReadAllText(filePath);
        return Parse(text);
    }

    public static FrozenDictionary<string, DeviceHidSpec> Parse(string tomlText)
    {
        var model = TomlSerializer.Deserialize(tomlText, DeviceHidSpecTomlContext.Default.TomlTable);
        if (model is null)
            return new Dictionary<string, DeviceHidSpec>().ToFrozenDictionary();

        var result = new Dictionary<string, DeviceHidSpec>();

        foreach (var kvp in model)
        {
            if (kvp.Value is not TomlTable deviceTable)
                continue;

            var name = kvp.Key;
            var hidId = ReadHidId(deviceTable);
            var axisScale = ReadAxisScale(deviceTable);
            var axesMappings = ReadAxesMappings(deviceTable);
            var buttonMappings = ReadButtonMappings(deviceTable);

            result[name] = new DeviceHidSpec(name, hidId, axesMappings, buttonMappings, axisScale);
        }

        return result.ToFrozenDictionary();
    }

    private static int[] ReadHidId(TomlTable deviceTable)
    {
        if (deviceTable.TryGetValue("hid_id", out var val) && val is TomlArray arr && arr.Count >= 2)
            return [(int)(long)arr[0]!, (int)(long)arr[1]!];
        return [0, 0];
    }

    private static double ReadAxisScale(TomlTable deviceTable)
    {
        if (deviceTable.TryGetValue("axis_scale", out var val))
        {
            if (val is long intVal) return intVal;
            if (val is double floatVal) return floatVal;
        }
        return 350.0;
    }

    private static FrozenDictionary<MotionAxis, AxisSpecs> ReadAxesMappings(TomlTable deviceTable)
    {
        var axes = new Dictionary<MotionAxis, AxisSpecs>();

        if (deviceTable.TryGetValue("mappings", out var mappingsObj) && mappingsObj is TomlTable mappingsTable)
        {
            foreach (var (key, axis) in AxisKeys)
            {
                if (mappingsTable.TryGetValue(key, out var arrObj) && arrObj is TomlArray arr && arr.Count >= 4)
                {
                    var flip = arr[3] is long i ? i : (double)arr[3]!;
                    axes[axis] = new AxisSpecs((byte)(long)arr[0]!, (int)(long)arr[1]!, (int)(long)arr[2]!, flip);
                }
            }
        }

        return axes.ToFrozenDictionary();
    }

    private static ReadOnlyCollection<ButtonSpecs> ReadButtonMappings(TomlTable deviceTable)
    {
        var buttons = new List<ButtonSpecs>();

        if (deviceTable.TryGetValue("buttons", out var btnObj) && btnObj is TomlTable btnTable)
        {
            foreach (var btnKvp in btnTable)
            {
                if (btnKvp.Value is TomlArray arr && arr.Count >= 3)
                    buttons.Add(new ButtonSpecs((byte)(long)arr[0]!, (int)(long)arr[1]!, (byte)(long)arr[2]!));
            }
        }

        return buttons.AsReadOnly();
    }
}

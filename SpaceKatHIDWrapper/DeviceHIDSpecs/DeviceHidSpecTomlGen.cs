using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

public static class DeviceHidSpecTomlGen
{
    public static void GenerateSpaceKatOnlyToml(string outputDir)
    {
        var spaceKatDevices = BuildSpaceKatDevices();
        var header =
            "# SpaceKatMotionMapper Device Specifications\n" +
            "# Generated: " + DateTime.Now.ToString("yyyy-MM-dd") + "\n" +
            "# Format:\n" +
            "#   hid_id = [vendor_id, product_id]\n" +
            "#   axis_scale = scaling factor (default 350.0)\n" +
            "#   mappings.<axis> = [channel, byte1, byte2, flip]\n" +
            "#   buttons.<NAME> = [channel, byte, bit]\n" +
            "#\n" +
            "# This file is embedded in the assembly and extracted on first run.\n" +
            "# To add a new device, add a new [DeviceName] section below.\n";

        using var writer = new StreamWriter(Path.Combine(outputDir, "devices.toml"));
        writer.Write(header);
        writer.WriteLine();
        foreach (var (key, spec) in spaceKatDevices)
        {
            writer.Write(DeviceHidSpecTomlWriter.WriteDevice(new KeyValuePair<string, DeviceHidSpec>(key, spec)));
            writer.WriteLine();
        }
    }

    public static void GenerateTomlFiles(string outputDir)
    {
        var spaceKatDevices = BuildSpaceKatDevices();
        var threeDConnexionDevices = Build3DConnexionDevices();

        var spaceKatHidIds = new HashSet<string>(
            spaceKatDevices.Values.Select(s => $"{s.HidId[0]:X4}:{s.HidId[1]:X4}"));

        var conflictDeviceNames = new HashSet<string>(
            threeDConnexionDevices.Where(kvp =>
                spaceKatHidIds.Contains($"{kvp.Value.HidId[0]:X4}:{kvp.Value.HidId[1]:X4}"))
                .Select(kvp => kvp.Key));

        WriteDevicesToml(outputDir, spaceKatDevices, threeDConnexionDevices, conflictDeviceNames);
        Write3DConnexionToml(outputDir, threeDConnexionDevices);
    }

    private static void WriteDevicesToml(string outputDir,
        Dictionary<string, DeviceHidSpec> spaceKatDevices,
        Dictionary<string, DeviceHidSpec> threeDConnexionDevices,
        HashSet<string> conflictNames)
    {
        var header =
            "# SpaceKatMotionMapper Device Specifications\n" +
            "# Generated: " + DateTime.Now.ToString("yyyy-MM-dd") + "\n" +
            "# Format:\n" +
            "#   hid_id = [vendor_id, product_id]\n" +
            "#   axis_scale = scaling factor (default 350.0)\n" +
            "#   mappings.<axis> = [channel, byte1, byte2, flip]\n" +
            "#   buttons.<NAME> = [channel, byte, bit]\n" +
            "#\n" +
            "# This file is embedded in the assembly and extracted on first run.\n" +
            "# To add a new device, add a new [DeviceName] section below.\n";

        using var writer = new StreamWriter(Path.Combine(outputDir, "devices.toml"));
        writer.Write(header);
        writer.WriteLine();

        writer.WriteLine("# ===== SpaceKat Devices =====");
        writer.WriteLine();
        foreach (var (key, spec) in spaceKatDevices)
        {
            writer.Write(DeviceHidSpecTomlWriter.WriteDevice(new KeyValuePair<string, DeviceHidSpec>(key, spec)));
            writer.WriteLine();
        }

        writer.WriteLine("# ===== 3DConnexion Devices =====");
        writer.WriteLine();
        foreach (var (key, spec) in threeDConnexionDevices)
        {
            if (conflictNames.Contains(key)) continue;
            writer.Write(DeviceHidSpecTomlWriter.WriteDevice(new KeyValuePair<string, DeviceHidSpec>(key, spec)));
            writer.WriteLine();
        }

        if (conflictNames.Count > 0)
        {
            writer.WriteLine("# ===== Conflict Information =====");
            writer.WriteLine("# The following 3DConnexion devices share VID:PID with SpaceKat devices.");
            writer.WriteLine("# SpaceKat entries take precedence. Original mappings preserved below:");
            writer.WriteLine();
            foreach (var key in conflictNames)
            {
                var spec = threeDConnexionDevices[key];
                var hidKey = $"{spec.HidId[0]:X4}:{spec.HidId[1]:X4}";
                var conflicting = spaceKatDevices
                    .First(x => $"{x.Value.HidId[0]:X4}:{x.Value.HidId[1]:X4}" == hidKey).Key;
                writer.WriteLine($"# CONFLICT: {key} [0x{spec.HidId[0]:X4}, 0x{spec.HidId[1]:X4}] overwritten by SpaceKat {conflicting}");
                foreach (var line in DeviceHidSpecTomlWriter.WriteDevice(new KeyValuePair<string, DeviceHidSpec>(key, spec)).Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        writer.WriteLine("# " + line.TrimEnd());
                }
                writer.WriteLine();
            }
        }
    }

    private static void Write3DConnexionToml(string outputDir,
        Dictionary<string, DeviceHidSpec> threeDConnexionDevices)
    {
        var header =
            "# 3DConnexion Device Specifications (Raw)\n" +
            "# Generated: " + DateTime.Now.ToString("yyyy-MM-dd") + "\n" +
            "#\n" +
            "# This file contains the original 3DConnexion device mappings\n" +
            "# WITHOUT SpaceKat overrides. Use this for reference.\n";

        using var writer = new StreamWriter(Path.Combine(outputDir, "devices_3dconnexion.toml"));
        writer.Write(header);
        writer.WriteLine();

        foreach (var (key, spec) in threeDConnexionDevices)
        {
            writer.Write(DeviceHidSpecTomlWriter.WriteDevice(new KeyValuePair<string, DeviceHidSpec>(key, spec)));
            writer.WriteLine();
        }
    }

    private static Dictionary<string, DeviceHidSpec> BuildSpaceKatDevices()
    {
        var baseAxes = new Dictionary<MotionAxis, AxisSpecs>
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1),
            [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary();

        var miniButtons = new List<ButtonSpecs> { new(3, 1, 0), new(3, 1, 1) }.AsReadOnly();
        var miniEButtons = new List<ButtonSpecs>
        {
            new(3, 4, 1), new(3, 1, 2), new(3, 1, 5),
            new(3, 1, 1), new(3, 1, 4), new(3, 2, 0),
        }.AsReadOnly();
        var proButtons = new List<ButtonSpecs>
        {
            new(3, 4, 1), new(3, 1, 2), new(3, 1, 5),
            new(3, 1, 1), new(3, 4, 2), new(3, 2, 0),
        }.AsReadOnly();

        return new Dictionary<string, DeviceHidSpec>
        {
            ["Mini"] = new("SpaceKat V3 Mini", [0x256F, 0xC635], baseAxes, miniButtons, 350.0),
            ["Mini_V1.3"] = new("SpaceKat V3 Mini", [0x256F, 0xC63A], baseAxes, miniButtons, 350.0),
            ["MiniE"] = new("SpaceKat V3 MiniE", [0x256F, 0xC638], baseAxes, miniEButtons, 350.0),
            ["MiniEWired"] = new("SpaceKat V3 MiniE Wired", [0x256F, 0xC631], baseAxes, miniEButtons, 350.0),
            ["ProDesigner"] = new("SpaceKatV3 Pro Designer", [0x256F, 0xC633], baseAxes, proButtons, 350.0),
        };
    }

    private static Dictionary<string, DeviceHidSpec> Build3DConnexionDevices()
    {
        var axesA = new Dictionary<MotionAxis, AxisSpecs>
        {
            [MotionAxis.X] = new(1, 1, 2, 1), [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1), [MotionAxis.Pitch] = new(2, 1, 2, -1),
            [MotionAxis.Roll] = new(2, 3, 4, -1), [MotionAxis.Yaw] = new(2, 5, 6, 1),
        }.ToFrozenDictionary();

        var axesB = new Dictionary<MotionAxis, AxisSpecs>
        {
            [MotionAxis.X] = new(1, 1, 2, 1), [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1), [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1), [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary();

        var proButtons = new List<ButtonSpecs>
        {
            new(3, 1, 0), new(3, 3, 7), new(3, 4, 1), new(3, 4, 0),
            new(3, 3, 6), new(3, 2, 4), new(3, 2, 5), new(3, 2, 6),
            new(3, 2, 7), new(3, 2, 0), new(3, 1, 2), new(3, 4, 2),
            new(3, 1, 5), new(3, 1, 4), new(3, 1, 1),
        }.AsReadOnly();

        return new Dictionary<string, DeviceHidSpec>
        {
            ["SpaceMouseEnterprise"] = new("SpaceMouseEnterprise", [0x256F, 0xC633], axesB,
                new List<ButtonSpecs> { new(3,2,4),new(3,2,5),new(3,2,6),new(3,2,7),new(3,3,0),new(3,3,1),new(3,3,2),new(3,3,3),new(3,3,4),new(3,3,5),new(3,1,0),new(3,1,1),new(3,1,2),new(3,1,4),new(3,1,5),new(3,2,0),new(3,2,2),new(3,3,6),new(3,3,7),new(3,4,0),new(3,4,1),new(3,4,2) }.AsReadOnly(), 350.0),

            ["SpaceExplorer"] = new("SpaceExplorer", [0x46D, 0xC627], axesA,
                new List<ButtonSpecs> { new(3,2,0),new(3,1,6),new(3,2,1),new(3,1,7),new(3,1,0),new(3,1,1),new(3,2,3),new(3,2,2),new(3,2,5),new(3,2,4),new(3,1,2),new(3,1,3),new(3,1,5),new(3,2,6),new(3,1,4) }.AsReadOnly(), 350.0),

            ["SpaceNavigator"] = new("SpaceNavigator", [0x46D, 0xC626], axesA,
                new List<ButtonSpecs> { new(3, 1, 0), new(3, 1, 1) }.AsReadOnly(), 350.0),

            ["SpaceMouseUSB"] = new("SpaceMouseUSB", [0x256F, 0xC641], axesA,
                new List<ButtonSpecs>().AsReadOnly(), 350.0),

            ["SpaceMouseCompact"] = new("SpaceMouseCompact", [0x256F, 0xC635], axesA,
                new List<ButtonSpecs> { new(3, 1, 0), new(3, 1, 1) }.AsReadOnly(), 350.0),

            ["SpaceMouseProWireless"] = new("SpaceMouseProWireless", [0x256F, 0xC632], axesB, proButtons, 350.0),

            ["SpaceMouseProWirelessBluetooth"] = new("SpaceMouseProWirelessBluetooth", [0x256F, 0xC638], axesB, proButtons, 350.0),

            ["SpaceMousePro"] = new("SpaceMousePro", [0x46D, 0xC62B], axesA, proButtons, 350.0),

            ["SpaceMouseWireless"] = new("SpaceMouseWireless", [0x256F, 0xC62E], axesB,
                new List<ButtonSpecs> { new(3, 1, 0), new(3, 1, 1) }.AsReadOnly(), 350.0),

            ["SpaceMouseWirelessNew"] = new("SpaceMouseWirelessNew", [0x256F, 0xC63A], axesB,
                new List<ButtonSpecs> { new(3, 1, 0), new(3, 1, 1) }.AsReadOnly(), 350.0),

            ["UniversalReceiver"] = new("UniversalReceiver", [0x256F, 0xC652], axesB, proButtons, 350.0),

            ["SpacePilot"] = new("SpacePilot", [0x46D, 0xC625], axesA,
                new List<ButtonSpecs> { new(3,1,0),new(3,1,1),new(3,1,2),new(3,1,3),new(3,1,4),new(3,1,5),new(3,1,6),new(3,1,7),new(3,2,0),new(3,2,1),new(3,2,2),new(3,2,3),new(3,2,4),new(3,2,5),new(3,2,6),new(3,2,7),new(3,3,0),new(3,3,1),new(3,3,2),new(3,3,3),new(3,3,4) }.AsReadOnly(), 350.0),

            ["SpacePilotPro"] = new("SpacePilotPro", [0x46D, 0xC629], axesA,
                new List<ButtonSpecs> { new(3,4,0),new(3,3,6),new(3,4,1),new(3,3,7),new(3,3,1),new(3,3,2),new(3,2,6),new(3,2,7),new(3,3,0),new(3,1,0),new(3,4,6),new(3,4,5),new(3,4,4),new(3,4,3),new(3,4,2),new(3,2,0),new(3,1,2),new(3,1,5),new(3,1,4),new(3,2,2),new(3,1,1) }.AsReadOnly(), 350.0),
        };
    }
}

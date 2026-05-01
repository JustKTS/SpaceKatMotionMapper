namespace SpaceKatHIDWrapper.Tests.DeviceHIDSpecs;

using SpaceKatHIDWrapper.DeviceHIDSpecs;
using TUnit.Core;

public class DeviceHidSpecDictTests
{
    [Test]
    public async Task Initialize_ThrowsOnMalformedToml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var badToml = "[Mini\nhid_id = [0x256F, 0xC635\naxis_scale = 350.0"; // malformed TOML
            await File.WriteAllTextAsync(Path.Combine(tempDir, "devices.toml"), badToml);

            var ex = Assert.Throws<InvalidOperationException>(
                () => DeviceHidSpecDict.Initialize(tempDir));

            await Assert.That(ex.Message).Contains("设备配置文件格式错误");
            await Assert.That(ex.Message).Contains(tempDir);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task ResetToDefault_RecoversFromCorruptedFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var badToml = "[Mini\nhid_id = [0x256F, 0xC635\naxis_scale = 350.0";
            await File.WriteAllTextAsync(Path.Combine(tempDir, "devices.toml"), badToml);

            Assert.Throws<InvalidOperationException>(
                () => DeviceHidSpecDict.Initialize(tempDir));

            DeviceHidSpecDict.ResetToDefault(tempDir);

            var specs = DeviceHidSpecDict.HidIds;
            await Assert.That(specs.Count).IsGreaterThan(0);
            await Assert.That(specs.ContainsKey("Mini")).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task Initialize_SucceedsWhenNoTomlExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            DeviceHidSpecDict.Initialize(tempDir);

            var specs = DeviceHidSpecDict.HidIds;
            await Assert.That(specs.Count).IsGreaterThan(0);
            await Assert.That(specs.ContainsKey("Mini")).IsTrue();
            await Assert.That(specs.ContainsKey("MiniE")).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task ReadFromFile_ParsesUserCreatedToml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var userToml =
                "[CustomDevice]\n" +
                "hid_id = [0x1234, 0x5678]\n" +
                "axis_scale = 400.0\n" +
                "mappings.x = [1, 1, 2, 1]\n" +
                "mappings.y = [1, 3, 4, -1]\n" +
                "mappings.z = [1, 5, 6, -1]\n" +
                "mappings.pitch = [2, 1, 2, -1]\n" +
                "mappings.roll = [2, 3, 4, -1]\n" +
                "mappings.yaw = [2, 5, 6, 1]\n";
            var tomlPath = Path.Combine(tempDir, "devices.toml");
            await File.WriteAllTextAsync(tomlPath, userToml);

            var specs = DeviceHidSpecTomlReader.ReadFromFile(tomlPath);

            await Assert.That(specs.Count).IsEqualTo(1);
            await Assert.That(specs.ContainsKey("CustomDevice")).IsTrue();
            await Assert.That(specs["CustomDevice"].AxisScale).IsEqualTo(400.0);
            await Assert.That(specs["CustomDevice"].HidId[0]).IsEqualTo(0x1234);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task ReadFromFile_ParsesPartialAxisMappings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var userToml =
                "[PartialDevice]\n" +
                "hid_id = [0x256F, 0xC641]\n" +
                "axis_scale = 350.0\n" +
                "mappings.x = [1, 1, 2, 1]\n" +
                "mappings.y = [1, 3, 4, -1]\n" +
                "mappings.z = [1, 5, 6, -1]\n";
            var tomlPath = Path.Combine(tempDir, "devices.toml");
            await File.WriteAllTextAsync(tomlPath, userToml);

            var specs = DeviceHidSpecTomlReader.ReadFromFile(tomlPath);

            await Assert.That(specs.Count).IsEqualTo(1);
            await Assert.That(specs["PartialDevice"].AxesMappings.Count).IsEqualTo(3);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task ResetToDefault_DeletesAndRecreatesDevicesToml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var badToml = "[Bad";
            await File.WriteAllTextAsync(Path.Combine(tempDir, "devices.toml"), badToml);

            DeviceHidSpecDict.ResetToDefault(tempDir);

            var specs = DeviceHidSpecDict.HidIds;
            await Assert.That(specs.Count).IsGreaterThan(0);
            await Assert.That(File.Exists(Path.Combine(tempDir, "devices.toml"))).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task Reload_With3DConnexion_ReadsDirectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            // Initialize creates SpaceKat toml
            DeviceHidSpecDict.Initialize(tempDir);
            var skSpecs = DeviceHidSpecDict.HidIds;
            await Assert.That(skSpecs.Count).IsEqualTo(5);

            // Reload with 3DConnexion reads that file directly
            DeviceHidSpecDict.Reload(tempDir, true);

            var specs = DeviceHidSpecDict.HidIds;
            await Assert.That(specs.Count).IsGreaterThan(5);
            await Assert.That(File.Exists(Path.Combine(tempDir, "devices_3dconnexion.toml"))).IsTrue();
            await Assert.That(specs.ContainsKey("SpaceExplorer")).IsTrue();
            await Assert.That(specs.ContainsKey("SpaceNavigator")).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task Reload_ToggleOffOn_SwitchesCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            DeviceHidSpecDict.Initialize(tempDir);
            await Assert.That(DeviceHidSpecDict.HidIds.Count).IsEqualTo(5);

            DeviceHidSpecDict.Reload(tempDir, true);
            await Assert.That(DeviceHidSpecDict.HidIds.Count).IsGreaterThan(5);

            DeviceHidSpecDict.Reload(tempDir, false);
            await Assert.That(DeviceHidSpecDict.HidIds.Count).IsEqualTo(5);
            await Assert.That(DeviceHidSpecDict.HidIds.ContainsKey("Mini")).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task Reload_ExtractsMissingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "hid_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            DeviceHidSpecDict.Initialize(tempDir);
            var connPath = Path.Combine(tempDir, "devices_3dconnexion.toml");

            // Delete conn file (simulate first toggle)
            File.Delete(connPath);
            await Assert.That(File.Exists(connPath)).IsFalse();

            // Reload should auto-extract
            DeviceHidSpecDict.Reload(tempDir, true);

            await Assert.That(File.Exists(connPath)).IsTrue();
            await Assert.That(DeviceHidSpecDict.HidIds.Count).IsGreaterThan(5);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

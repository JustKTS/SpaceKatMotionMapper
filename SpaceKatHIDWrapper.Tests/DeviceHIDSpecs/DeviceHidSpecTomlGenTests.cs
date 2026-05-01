using SpaceKatHIDWrapper.DeviceHIDSpecs;
using TUnit.Assertions;
using TUnit.Core;

namespace SpaceKatHIDWrapper.Tests.DeviceHIDSpecs;

public class DeviceHidSpecTomlGenTests
{
    [Test]
    public async Task GenerateTomlFiles_CreatesBothFiles()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "device_toml_gen_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDir);

        try
        {
            DeviceHidSpecTomlGen.GenerateTomlFiles(outputDir);

            await Assert.That(File.Exists(Path.Combine(outputDir, "devices.toml"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(outputDir, "devices_3dconnexion.toml"))).IsTrue();

            var devicesToml = await File.ReadAllTextAsync(Path.Combine(outputDir, "devices.toml"));
            await Assert.That(devicesToml).Contains("Mini");
            await Assert.That(devicesToml).Contains("SpaceExplorer");
            await Assert.That(devicesToml).Contains("CONFLICT");

            var connToml = await File.ReadAllTextAsync(Path.Combine(outputDir, "devices_3dconnexion.toml"));
            await Assert.That(connToml).Contains("SpaceMouseCompact");
            await Assert.That(connToml).Contains("3DConnexion");
        }
        finally
        {
            Directory.Delete(outputDir, true);
        }
    }
}

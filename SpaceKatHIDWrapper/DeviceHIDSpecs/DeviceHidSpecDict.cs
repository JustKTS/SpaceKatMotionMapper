using System.Collections.Frozen;
using System.Reflection;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

public static class DeviceHidSpecDict
{
    private const string EmbeddedResourceName = "SpaceKatHIDWrapper.DeviceHIDSpecs.devices.toml";
    private const string EmbeddedThreeDConnexionResourceName = "SpaceKatHIDWrapper.DeviceHIDSpecs.devices_3dconnexion.toml";
    private const string DevicesFileName = "devices.toml";
    private const string ThreeDConnexionFileName = "devices_3dconnexion.toml";

    private static FrozenDictionary<string, DeviceHidSpec>? _hidIds;

    public static FrozenDictionary<string, DeviceHidSpec> HidIds =>
        _hidIds ?? throw new InvalidOperationException(
            $"{nameof(DeviceHidSpecDict)} not initialized. Call {nameof(Initialize)} first.");

    public static void Initialize(string configDirPath)
    {
        if (!Directory.Exists(configDirPath))
            Directory.CreateDirectory(configDirPath);

        var tomlPath = Path.Combine(configDirPath, DevicesFileName);
        if (!File.Exists(tomlPath))
            ExtractEmbeddedResource(EmbeddedResourceName, tomlPath);

        try
        {
            _hidIds = DeviceHidSpecTomlReader.ReadFromFile(tomlPath);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"设备配置文件格式错误: {tomlPath}\n{ex.Message}\n可以删除此文件让程序从内置资源恢复默认配置",
                ex);
        }
    }

    public static void Reload(string configDirPath, bool includeThreeDConnexion)
    {
        var fileName = includeThreeDConnexion ? ThreeDConnexionFileName : DevicesFileName;
        var resourceName = includeThreeDConnexion ? EmbeddedThreeDConnexionResourceName : EmbeddedResourceName;
        var filePath = Path.Combine(configDirPath, fileName);

        if (!File.Exists(filePath))
            ExtractEmbeddedResource(resourceName, filePath);

        try
        {
            _hidIds = DeviceHidSpecTomlReader.ReadFromFile(filePath);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"设备配置文件格式错误: {filePath}\n{ex.Message}\n可以删除此文件让程序从内置资源恢复默认配置",
                ex);
        }
    }

    public static void ResetToDefault(string configDirPath, bool includeThreeDConnexion = false)
    {
        var fileName = includeThreeDConnexion ? ThreeDConnexionFileName : DevicesFileName;
        var tomlPath = Path.Combine(configDirPath, DevicesFileName);
        var connPath = Path.Combine(configDirPath, ThreeDConnexionFileName);

        if (includeThreeDConnexion)
        {
            if (File.Exists(connPath)) File.Delete(connPath);
            Reload(configDirPath, true);
        }
        else
        {
            if (File.Exists(tomlPath)) File.Delete(tomlPath);
            if (File.Exists(connPath)) File.Delete(connPath);
            Initialize(configDirPath);
        }
    }

    private static void ExtractEmbeddedResource(string resourceName, string targetPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new FileNotFoundException(
                $"Embedded resource '{resourceName}' not found in assembly '{assembly.FullName}'.");

        using var fileStream = File.Create(targetPath);
        stream.CopyTo(fileStream);
    }
}

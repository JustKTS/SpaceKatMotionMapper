using System;
using System.IO;

namespace SpaceKat.Shared.States;

public static class GlobalPaths
{
    private static string? _appFolderName;

    public static void Initialize(string appFolderName)
    {
        _appFolderName = appFolderName;
    }

    private static string AppFolderName => _appFolderName
        ?? throw new InvalidOperationException("GlobalPaths must be initialized with a folder name before first use.");

    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    public static string AppLogPath => Path.Combine(AppDataPath, "Logs");
    
    public static string MetaKeysConfigPath => Path.Combine(AppDataPath, "ProgramSpecificMetaKeys");
    
    public static string DownloadTempDir => Path.Combine(AppDataPath, "DownloadTemp");
}
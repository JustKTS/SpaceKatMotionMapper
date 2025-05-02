namespace SpaceKat.Shared.States;

public static class GlobalPaths
{
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SpaceKatMotionMapper");

    public static string AppLogPath => Path.Combine(AppDataPath, "Logs");
    
    public static string MetaKeysConfigPath => Path.Combine(AppDataPath, "ProgramSpecificMetaKeys");
    
    public static string DownloadTempDir => Path.Combine(AppDataPath, "DownloadTemp");
}
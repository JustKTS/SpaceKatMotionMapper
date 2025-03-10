using System;
using System.IO;

namespace SpaceKatMotionMapper.States;

public static class GlobalPaths
{
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        nameof(SpaceKatMotionMapper));
    public static string AppLogPath => Path.Combine(AppDataPath, "Logs");
}
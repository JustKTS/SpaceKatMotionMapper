using System;

namespace PlatformAbstractions;

public static class PlatformHelper
{
    public static bool IsWindows() => OperatingSystem.IsWindows();
    public static bool IsLinux() => OperatingSystem.IsLinux();
    // ReSharper disable once InconsistentNaming
    public static bool IsMacOS() => OperatingSystem.IsMacOS();

    public static string GetPlatformUnsupportedMessage(string featureName)
        => $"{featureName} 功能仅在 Windows 平台上支持。";
}
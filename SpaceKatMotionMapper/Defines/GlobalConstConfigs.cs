namespace SpaceKatMotionMapper.Defines;

public static class GlobalConstConfigs
{
    public const int ConfigFileVersion = 4;
    public static string SoftVersion { get; } =
        typeof(GlobalConstConfigs).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
}
using System.Diagnostics;

namespace LinuxHelpers.Services.Platform;

public static class LinuxScaleDetector
{
    public static void DetectAndApply()
    {
        var scale = DetectSystemScale();
        if (scale is > 1.0)
            ApplyScaleViaXrdb(scale.Value);
    }

    internal static double? DetectSystemScale()
    {
        return DetectFromNiri()
               ?? DetectFromXrdb()
               ?? DetectFromEnvVars();
    }

    private static void ApplyScaleViaXrdb(double scale)
    {
        var dpi = (int)(scale * 96);
        try
        {
            var psi = new ProcessStartInfo("xrdb", $"-merge")
            {
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process is null) return;
            process.StandardInput.Write($"Xft.dpi: {dpi}\n");
            process.StandardInput.Close();
            process.WaitForExit(2000);
        }
        catch
        {
            // xrdb not available or failed, ignore
        }
    }

    internal static double? DetectFromXrdb()
    {
        try
        {
            var psi = new ProcessStartInfo("xrdb", "-query")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process is null) return null;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);

            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("Xft.dpi:")) continue;
                var dpiStr = trimmed["Xft.dpi:".Length..].Trim();
                if (double.TryParse(dpiStr, out var dpi) && dpi > 96)
                    return dpi / 96.0;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    internal static double? DetectFromNiri()
    {
        try
        {
            var psi = new ProcessStartInfo("niri", "msg outputs")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process is null) return null;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);

            double? maxScale = null;
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("Scale:")) continue;
                var scaleStr = trimmed["Scale:".Length..].Trim();
                if (double.TryParse(scaleStr, out var scale) && scale > 1.0)
                {
                    if (maxScale is null || scale > maxScale)
                        maxScale = scale;
                }
            }

            return maxScale;
        }
        catch
        {
            return null;
        }
    }

    internal static double? DetectFromEnvVars()
    {
        var gdkScale = Environment.GetEnvironmentVariable("GDK_SCALE");
        if (int.TryParse(gdkScale, out var gs) && gs > 1)
            return gs;

        var qtScale = Environment.GetEnvironmentVariable("QT_SCALE_FACTOR");
        if (double.TryParse(qtScale, out var qs) && qs > 1.0)
            return qs;

        return null;
    }
}

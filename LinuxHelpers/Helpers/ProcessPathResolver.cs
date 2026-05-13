using System.Diagnostics;

namespace LinuxHelpers.Helpers;

public static class LinuxProcessPathResolver
{
    private const int CacheTtlMs = 5000;

    private static readonly Dictionary<string, (string Path, DateTime Timestamp)> Cache = new();

    private static readonly Dictionary<string, string[]> AppIdMappings = new()
    {
        { "zen", ["zen-browser", "zen"] },
        { "code", ["code", "code-oss", "code-insiders", "codium"] },
        { "code-oss", ["code-oss", "code"] },
        { "chrome", ["chrome", "google-chrome", "google-chrome-stable", "google-chrome-beta"] },
        { "firefox", ["firefox", "firefox-bin", "firefox-trunk"] },
        { "QQ", ["qq", "linuxqq", "QQ", "com.qq.QQ"] },
        { "wechat", ["wechat", "WeChat", "com.tencent.WeChat"] },
        { "telegram", ["telegram-desktop", "telegram"] },
        { "discord", ["discord", "discord-canary", "discord-ptb"] },
        { "spotify", ["spotify", "spotify-client"] },
        { "thunderbird", ["thunderbird", "thunderbird-bin"] },
        { "vlc", ["vlc", "vlc-bin"] },
    };

    public static string GetExecutablePathFromAppId(string appId, string title)
    {
        var key = appId.ToLower().Trim();

        if (Cache.TryGetValue(key, out var cached) && (DateTime.Now - cached.Timestamp).TotalMilliseconds < CacheTtlMs)
            return cached.Path;

        var path = PerformLookup(key, title.Trim());
        Cache[key] = (path, DateTime.Now);
        return path;
    }

    private static string PerformLookup(string appId, string title)
    {
        // Strategy 1: pgrep appId
        var pid = Pgrep(appId);
        if (pid > 0)
        {
            var path = ReadProcExe(pid);
            if (!string.IsNullOrEmpty(path)) return path;
        }

        // Strategy 2: try known mappings
        if (AppIdMappings.TryGetValue(appId, out var processNames))
        {
            foreach (var name in processNames)
            {
                pid = Pgrep(name);
                if (pid > 0)
                {
                    var path = ReadProcExe(pid);
                    if (!string.IsNullOrEmpty(path)) return path;
                }
            }
        }

        // Strategy 3: try pgrep -f (fuzzy match)
        pid = Pgrep(appId);
        if (pid > 0)
        {
            var path = ReadProcExe(pid);
            if (!string.IsNullOrEmpty(path)) return path;
        }

        // Strategy 4: extract candidate from title
        if (!string.IsNullOrEmpty(title))
        {
            foreach (var part in title.Split(' ', '-', '—', '–').Take(3))
            {
                if (part.Length < 2) continue;
                pid = Pgrep(part);
                if (pid > 0)
                {
                    var path = ReadProcExe(pid);
                    if (!string.IsNullOrEmpty(path)) return path;
                }
            }
        }

        return string.Empty;
    }

    private static int Pgrep(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return -1;
        try
        {
            var args = $"-of \"{pattern}\"";
            var psi = new ProcessStartInfo("pgrep", args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return -1;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(1000);

            return process.ExitCode == 0 && int.TryParse(output.Trim(), out var pid) ? pid : -1;
        }
        catch
        {
            return -1;
        }
    }

    private static string? ReadProcExe(int pid)
    {
        if (pid <= 0) return null;
        try
        {
            var psi = new ProcessStartInfo("readlink", $"-f /proc/{pid}/exe")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(1000);

            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}

using System.Diagnostics;

namespace LinuxHelpers.Helpers;

public static class LinuxProcessPathResolver
{
    private const int CacheTtlMs = 5000;
    private const int CommandTimeoutMs = 1000;

    private static readonly Dictionary<string, (string Path, DateTime Timestamp)> Cache = new();

    private const int MaxProcNameLen = 15;

    private static readonly HashSet<string> KnownShellPaths = new(StringComparer.Ordinal)
    {
        "/usr/bin/bash", "/bin/bash",
        "/usr/bin/sh",   "/bin/sh",
        "/usr/bin/dash", "/bin/dash",
        "/usr/bin/zsh",  "/bin/zsh",
        "/usr/bin/fish", "/bin/fish",
    };

    private static readonly string[] KnownBinaryDirs =
    [
        "/usr/bin", "/usr/local/bin", "/bin", "/snap/bin",
    ];

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
        bool TryGetValidPath(int pid, out string path)
        {
            path = string.Empty;
            if (pid <= 0) return false;
            var raw = ReadProcExe(pid);
            if (string.IsNullOrEmpty(raw)) return false;
            if (IsShellPath(raw)) return false;
            path = raw;
            return true;
        }

        // S1: pgrep appId — picks -nx (safe) vs -nf automatically based on name length
        if (TryGetValidPath(Pgrep(appId, preferExactName: true), out var path)) return path;

        // S2: try AppIdMappings with pgrep (exact-name first, then cmdline)
        if (AppIdMappings.TryGetValue(appId, out var processNames))
        {
            foreach (var name in processNames)
            {
                if (TryGetValidPath(Pgrep(name, preferExactName: true), out path)) return path;
                if (TryGetValidPath(Pgrep(name, preferExactName: false), out path)) return path;
            }
        }

        // S3: full cmdline match on appId (fallback for long appIds that can only match via -f)
        if (TryGetValidPath(Pgrep(appId, preferExactName: false), out path)) return path;

        // S4: resolve binary path via which + known directories
        path = ResolveBinaryPath(appId, title);
        if (!string.IsNullOrEmpty(path)) return path;

        return string.Empty;
    }

    private static string? ResolveBinaryPath(string appId, string title)
    {
        // Collect candidates: AppIdMappings names + appId itself (deduplicated)
        var candidates = new List<string>();
        if (AppIdMappings.TryGetValue(appId, out var mapped))
            candidates.AddRange(mapped.Where(n => !candidates.Contains(n)));
        if (!candidates.Contains(appId))
            candidates.Add(appId);

        // Try which first, then known directories
        foreach (var name in candidates)
        {
            var p = Which(name);
            if (!string.IsNullOrEmpty(p) && !IsShellPath(p)) return p;
            p = TryFindInKnownDirs(name);
            if (!string.IsNullOrEmpty(p)) return p;
        }

        // Title-based candidates: extract first words from title
        if (!string.IsNullOrEmpty(title))
        {
            foreach (var part in title.Split(' ', '-', '\u2014', '\u2013', '|'))
            {
                var clean = part.Trim();
                if (clean.Length < 2) continue;
                if (clean.Contains('.', StringComparison.Ordinal)) continue;
                var lower = clean.ToLowerInvariant();
                var p = Which(lower);
                if (!string.IsNullOrEmpty(p) && !IsShellPath(p)) return p;
                p = TryFindInKnownDirs(lower);
                if (!string.IsNullOrEmpty(p)) return p;
            }
        }

        return null;
    }

    private static string? TryFindInKnownDirs(string name)
    {
        foreach (var dir in KnownBinaryDirs)
        {
            var fullPath = Path.Combine(dir, name);
            if (File.Exists(fullPath))
                return fullPath;
        }
        return null;
    }

    /// <summary>
    /// Run pgrep to locate a process PID.
    /// When preferExactName is true and the pattern fits within the kernel's 15-char
    /// process-name limit, uses -nx (exact name, newest). Otherwise degrades to -nf
    /// (full command line match, newest) silently.
    /// </summary>
    private static int Pgrep(string pattern, bool preferExactName = true)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return -1;
        try
        {
            // Kernel truncates process names (comm) to 15 chars; -nx is doomed on longer patterns
            var useExact = preferExactName && pattern.Length <= MaxProcNameLen;
            var flag = useExact ? "-nx" : "-nf";
            var args = $"{flag} \"{pattern}\"";
            var psi = new ProcessStartInfo("pgrep", args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return -1;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(CommandTimeoutMs);

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
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(CommandTimeoutMs);

            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsShellPath(string? path)
    {
        return path != null && KnownShellPaths.Contains(path);
    }

    private static string? Which(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        try
        {
            var psi = new ProcessStartInfo("which", name)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(CommandTimeoutMs);

            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}

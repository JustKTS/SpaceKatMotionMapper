using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Serilog;

namespace LinuxHelpers.Helpers;

/// <summary>
/// Linux 平台进程路径解析器
/// 通过 app-id 和窗口标题查找进程的可执行文件路径
/// </summary>
public class LinuxProcessPathResolver
{
    private const int CacheTtlMs = 5000; // 5秒缓存
    private const int MaxCacheSize = 50;
    private const int CommandTimeoutMs = 1000; // 命令超时1秒

    private readonly ConcurrentDictionary<string, CachedPathResult> _pathCache = new();

    /// <summary>
    /// app-id 到进程名的已知映射表
    /// 用于处理 app-id 与实际进程名不一致的情况
    /// </summary>
    private static readonly Dictionary<string, string[]> AppIdProcessMappings = new()
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

    /// <summary>
    /// 从 app-id 和窗口标题获取可执行文件路径
    /// </summary>
    /// <param name="appId">应用标识符（来自 lswt）</param>
    /// <param name="title">窗口标题</param>
    /// <returns>可执行文件的完整路径，如果找不到则返回空字符串</returns>
    public string GetExecutablePathFromAppId(string appId, string title)
    {
        // 规范化输入
        var normalizedAppId = NormalizeCacheKey(appId);
        var normalizedTitle = title.Trim();

        // 如果 app-id 为空，尝试从标题提取
        if (string.IsNullOrEmpty(normalizedAppId) && !string.IsNullOrEmpty(normalizedTitle))
        {
            var pidFromTitle = FindPidByTitle(normalizedTitle);
            if (!pidFromTitle.HasValue) return string.Empty;
            var path = ReadProcExe(pidFromTitle.Value);
            return !string.IsNullOrEmpty(path) ? path : string.Empty;
        }

        // 检查缓存
        if (_pathCache.TryGetValue(normalizedAppId, out var cached))
        {
            if ((DateTime.Now - cached.Timestamp).TotalMilliseconds < CacheTtlMs)
            {
                return cached.Path;
            }
            // 缓存过期，移除
            _pathCache.TryRemove(normalizedAppId, out _);
        }

        // 执行查找
        var resultPath = PerformLookup(normalizedAppId, normalizedTitle);

        // 缓存结果
        CacheResult(normalizedAppId, resultPath);

        return resultPath;
    }

    /// <summary>
    /// 执行实际的路径查找
    /// </summary>
    private string PerformLookup(string appId, string title)
    {
        // 策略1: 通过 app-id 直接查找
        var pid = FindPidByAppId(appId);
        if (pid.HasValue)
        {
            var path = ReadProcExe(pid.Value);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
        }

        // 策略2: 通过标题查找
        if (!string.IsNullOrEmpty(title))
        {
            var pidFromTitle = FindPidByTitle(title);
            if (pidFromTitle.HasValue)
            {
                var path = ReadProcExe(pidFromTitle.Value);
                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }
        }

        Log.Warning("[{Service}] No path found for: {AppId}", nameof(LinuxProcessPathResolver), appId);
        return string.Empty;
    }

    /// <summary>
    /// 通过 app-id 查找进程 PID
    /// </summary>
    private int? FindPidByAppId(string appId)
    {
        if (string.IsNullOrEmpty(appId)) return null;

        // 尝试直接匹配
        var pid = ExecutePgrep(appId, matchFullCommand: false);
        if (pid.HasValue)
        {
            return pid;
        }

        // 尝试已知映射表
        if (AppIdProcessMappings.TryGetValue(appId, out var processNames))
        {
            foreach (var processName in processNames)
            {
                pid = ExecutePgrep(processName, matchFullCommand: false);
                if (pid.HasValue)
                {
                    return pid;
                }
            }
        }

        // 尝试模糊匹配（匹配完整命令行）
        pid = ExecutePgrep(appId, matchFullCommand: true);
        if (pid.HasValue)
        {
            return pid;
        }

        return null;
    }

    /// <summary>
    /// 通过窗口标题查找进程 PID
    /// </summary>
    private int? FindPidByTitle(string title)
    {
        if (string.IsNullOrEmpty(title)) return null;

        // 从标题中提取可能的进程名
        // 策略: 取标题的第一个单词（通常是应用名）
        var parts = title.Split([' ', '-', '—', '–'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        // 尝试标题的前几个部分
        for (int i = 0; i < Math.Min(3, parts.Length); i++)
        {
            var candidate = parts[i].ToLower().Trim();
            // 跳过太短的候选词
            if (candidate.Length < 2) continue;

            var pid = ExecutePgrep(candidate, matchFullCommand: false);
            if (pid.HasValue)
            {
                return pid;
            }
        }

        // 尝试从标题中提取常见的应用模式
        // 例如: "Mozilla Firefox" -> "firefox"
        var titleLower = title.ToLower();
        foreach (var mapping in AppIdProcessMappings)
        {
            foreach (var processName in mapping.Value)
            {
                if (titleLower.Contains(processName.ToLower()))
                {
                    var pid = ExecutePgrep(processName, matchFullCommand: false);
                    if (pid.HasValue)
                    {
                        return pid;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 执行 pgrep 命令查找进程 PID
    /// </summary>
    /// <param name="pattern">搜索模式</param>
    /// <param name="matchFullCommand">是否匹配完整命令行（-f 参数）</param>
    /// <returns>找到的 PID，如果未找到则返回 null</returns>
    private int? ExecutePgrep(string pattern, bool matchFullCommand = false)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return null;

        try
        {
            var arguments = new StringBuilder();
            arguments.Append("-o"); // 只返回最老的 PID

            if (matchFullCommand)
            {
                arguments.Append(" -f");
            }

            arguments.Append($" \"{pattern}\"");

            var startInfo = new ProcessStartInfo
            {
                FileName = "pgrep",
                Arguments = arguments.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            // 等待进程完成，设置超时
            var exited = process.WaitForExit(CommandTimeoutMs);

            if (!exited)
            {
                Log.Warning("[{Service}] pgrep timeout for pattern: {Pattern}", nameof(LinuxProcessPathResolver), pattern);
                try
                {
                    process.Kill();
                }
                catch
                {
                    // 忽略杀死进程时的错误
                }
                return null;
            }

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var outputTrimmed = output.Trim();
                if (int.TryParse(outputTrimmed, out var pid))
                {
                    return pid;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Service}] pgrep exception for pattern '{Pattern}': {Message}",
                nameof(LinuxProcessPathResolver), pattern, ex.Message);
        }

        return null;
    }

    /// <summary>
    /// 读取 /proc/[pid]/exe 符号链接获取可执行文件路径
    /// </summary>
    /// <param name="pid">进程 ID</param>
    /// <returns>可执行文件的完整路径，如果读取失败则返回 null</returns>
    private string? ReadProcExe(int pid)
    {
        if (pid <= 0) return null;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "readlink",
                Arguments = $"-f /proc/{pid}/exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            var exited = process.WaitForExit(CommandTimeoutMs);

            if (!exited)
            {
                Log.Warning("[{Service}] readlink timeout for PID: {Pid}", nameof(LinuxProcessPathResolver), pid);
                try
                {
                    process.Kill();
                }
                catch
                {
                    // 忽略杀死进程时的错误
                }
                return null;
            }

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var path = output.Trim();
                return path;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Service}] readlink exception for PID {Pid}: {Message}",
                nameof(LinuxProcessPathResolver), pid, ex.Message);
        }

        return null;
    }

    /// <summary>
    /// 规范化缓存键
    /// </summary>
    private static string NormalizeCacheKey(string appId)
    {
        return appId.ToLower().Trim();
    }

    /// <summary>
    /// 缓存查找结果
    /// </summary>
    private void CacheResult(string appId, string path)
    {
        // 如果缓存已满，移除最旧的条目
        if (_pathCache.Count >= MaxCacheSize)
        {
            var oldest = _pathCache.OrderBy(kvp => kvp.Value.Timestamp).FirstOrDefault();
            if (oldest.Key is { } key)
            {
                _pathCache.TryRemove(key, out _);
            }
        }

        _pathCache[appId] = new CachedPathResult
        {
            Path = path,
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// 缓存结果记录
    /// </summary>
    private class CachedPathResult
    {
        public string Path { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

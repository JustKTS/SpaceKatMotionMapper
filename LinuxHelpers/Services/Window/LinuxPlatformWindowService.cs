using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using PlatformAbstractions;
using Serilog;
using LinuxHelpers.Services.Window.Lswt;
using LinuxHelpers.Helpers;

namespace LinuxHelpers.Services.Window;

/// <summary>
/// Linux平台的窗口信息服务实现
/// 使用lswt工具获取Wayland/sway环境下的窗口信息
/// </summary>
public class LinuxPlatformWindowService : IPlatformWindowService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<LinuxPlatformWindowService>();

    private const string LswtCommand = "lswt";
    private const int CommandTimeoutMs = 5000;
    private static bool? _isLswtAvailable;

    public IReadOnlyList<ForeProgramInfo> FindAllForegroundPrograms()
    {
        if (!IsLswtAvailable())
        {
            return new List<ForeProgramInfo>
            {
                new("Linux 平台需要安装 lswt 工具",
                    "请安装 lswt 以获取窗口列表",
                    "仅支持 sway/wlroots Wayland 合成器",
                    "")
            };
        }

        try
        {
            var jsonOutput = ExecuteLswtCommand();
            if (string.IsNullOrEmpty(jsonOutput))
                return Array.Empty<ForeProgramInfo>();

            var response = JsonSerializer.Deserialize(
                jsonOutput, LswtJsonContext.Default.LswtResponse);

            if (response?.Toplevels == null)
                return Array.Empty<ForeProgramInfo>();

            return ConvertToplevelsToForeProgramInfo(response.Toplevels);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get Linux window list");
            return Array.Empty<ForeProgramInfo>();
        }
    }

    public async IAsyncEnumerable<ForeProgramInfo> FindAllForegroundProgramsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var info in FindAllForegroundPrograms())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return info;
        }
        await Task.CompletedTask;
    }

    private static bool IsLswtAvailable()
    {
        if (_isLswtAvailable.HasValue)
            return _isLswtAvailable.Value;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = LswtCommand,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                _isLswtAvailable = false;
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            _isLswtAvailable = !string.IsNullOrEmpty(output.Trim());
            return _isLswtAvailable.Value;
        }
        catch
        {
            _isLswtAvailable = false;
            return false;
        }
    }

    private static string? ExecuteLswtCommand()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = LswtCommand,
                Arguments = "-j",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (process.WaitForExit(CommandTimeoutMs))
            {
                if (process.ExitCode != 0)
                {
                    Log.Error("lswt failed (exit {ExitCode}): {Error}",
                        process.ExitCode, errorTask.Result);
                    return null;
                }
                return outputTask.Result;
            }

            try { process.Kill(); } catch { }
            Log.Warning("lswt timeout");
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "lswt error");
            return null;
        }
    }

    private IReadOnlyList<ForeProgramInfo> ConvertToplevelsToForeProgramInfo(List<LswtToplevel> toplevels)
    {
        var seen = new HashSet<string>();

        return (from t in toplevels
            where !t.Minimized
            let name = string.IsNullOrEmpty(t.AppId) ? GetNameFromTitle(t.Title) : t.AppId
            where seen.Add(name)
            let path = LinuxProcessPathResolver.GetExecutablePathFromAppId(t.AppId ?? "", t.Title ?? "")
            select new ForeProgramInfo(t.Title ?? "", name, "", path)).ToList();
    }

    private static string GetNameFromTitle(string? title)
    {
        if (string.IsNullOrEmpty(title)) return "unknown";
        var parts = title.Split([' ', '-', '—'], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "unknown";
    }
}

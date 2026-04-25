using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
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
    private const string LswtCommand = "lswt";
    private const int CommandTimeoutMs = 5000;
    private static bool? _isLswtAvailable;
    private readonly LinuxProcessPathResolver _processPathResolver = new();

    /// <summary>
    /// 获取所有前台程序信息（同步版本）
    /// </summary>
    public IReadOnlyList<ForeProgramInfo> FindAllForegroundPrograms()
    {
        Log.Debug("[{Service}] FindAllForegroundPrograms() called", nameof(LinuxPlatformWindowService));

        if (!IsLswtAvailable())
        {
            Log.Warning("[{Service}] lswt tool not available, returning prompt info", nameof(LinuxPlatformWindowService));
            // 返回提示信息，告诉用户需要安装 lswt
            return new List<ForeProgramInfo>
            {
                new ForeProgramInfo(
                    "Linux 平台需要安装 lswt 工具",
                    "请安装 lswt 以获取窗口列表",
                    "仅支持 sway/wlroots Wayland 合成器",
                    ""
                )
            };
        }

        Log.Debug("[{Service}] lswt tool available, executing command", nameof(LinuxPlatformWindowService));
        try
        {
            var jsonOutput = ExecuteLswtCommand();
            if (string.IsNullOrEmpty(jsonOutput))
            {
                Log.Warning("[{Service}] lswt command returned empty output", nameof(LinuxPlatformWindowService));
                return Array.Empty<ForeProgramInfo>();
            }

            Log.Debug("[{Service}] lswt returned data length: {Length}", nameof(LinuxPlatformWindowService), jsonOutput.Length);
            var response = JsonSerializer.Deserialize(
                jsonOutput,
                LswtJsonContext.Default.LswtResponse);

            if (response?.Toplevels == null)
            {
                Log.Warning("[{Service}] Deserialization failed or toplevels is empty", nameof(LinuxPlatformWindowService));
                return Array.Empty<ForeProgramInfo>();
            }

            Log.Information("[{Service}] Found {Count} windows", nameof(LinuxPlatformWindowService), response.Toplevels.Count);
            return ConvertToplevelsToForeProgramInfo(response.Toplevels);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Service}] Failed to get Linux window list: {Message}", nameof(LinuxPlatformWindowService), ex.Message);
            return Array.Empty<ForeProgramInfo>();
        }
    }

    /// <summary>
    /// 异步枚举所有前台程序信息
    /// </summary>
    public async IAsyncEnumerable<ForeProgramInfo> FindAllForegroundProgramsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsLswtAvailable())
        {
            // 返回提示信息，告诉用户需要安装 lswt
            yield return new ForeProgramInfo(
                "Linux 平台需要安装 lswt 工具",
                "请安装 lswt 以获取窗口列表",
                "仅支持 sway/wlroots Wayland 合成器",
                ""
            );
            yield break;
        }

        var channel = Channel.CreateUnbounded<ForeProgramInfo>();

        // 在后台线程中执行窗口枚举
        await Task.Run(() =>
        {
            try
            {
                var jsonOutput = ExecuteLswtCommand();
                if (string.IsNullOrEmpty(jsonOutput))
                {
                    return;
                }

                var response = JsonSerializer.Deserialize(
                    jsonOutput,
                    LswtJsonContext.Default.LswtResponse);

                if (response?.Toplevels == null)
                {
                    return;
                }

                foreach (var info in ConvertToplevelsToForeProgramInfo(response.Toplevels))
                {
                    channel.Writer.TryWrite(info);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // 异步流式接收数据
        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return item;
        }
    }

    /// <summary>
    /// 检查lswt工具是否可用
    /// </summary>
    private static bool IsLswtAvailable()
    {
        if (_isLswtAvailable.HasValue)
        {
            Log.Debug("[{Service}] IsLswtAvailable() (cached): {Available}", nameof(LinuxPlatformWindowService), _isLswtAvailable.Value);
            return _isLswtAvailable.Value;
        }

        Log.Debug("[{Service}] IsLswtAvailable() starting check", nameof(LinuxPlatformWindowService));
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = LswtCommand,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                Log.Warning("[{Service}] which process failed to start", nameof(LinuxPlatformWindowService));
                _isLswtAvailable = false;
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            _isLswtAvailable = !string.IsNullOrEmpty(output.Trim());
            Log.Debug("[{Service}] which returned: '{Output}', error: '{Error}', available: {Available}",
                nameof(LinuxPlatformWindowService), output.Trim(), error, _isLswtAvailable.Value);
            return _isLswtAvailable.Value;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Service}] Exception while checking lswt: {Message}", nameof(LinuxPlatformWindowService), ex.Message);
            _isLswtAvailable = false;
            return false;
        }
    }

    /// <summary>
    /// 执行lswt -j命令并获取JSON输出
    /// </summary>
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

            // 使用超时等待，避免进程挂起
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (process.WaitForExit(CommandTimeoutMs))
            {
                var output = outputTask.Result;
                var error = errorTask.Result;

                if (process.ExitCode != 0)
                {
                    Log.Error("[{Service}] lswt command execution failed (exit code: {ExitCode}): {Error}",
                        nameof(LinuxPlatformWindowService), process.ExitCode, error);
                    return null;
                }

                return output;
            }
            else
            {
                // 超时，杀死进程
                try
                {
                    process.Kill();
                }
                catch
                {
                    // 忽略杀死进程时的异常
                }
                Log.Warning("[{Service}] lswt command execution timeout", nameof(LinuxPlatformWindowService));
                return null;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Service}] Exception while executing lswt command: {Message}",
                nameof(LinuxPlatformWindowService), ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 将lswt的toplevel列表转换为ForeProgramInfo列表
    /// </summary>
    private IReadOnlyList<ForeProgramInfo> ConvertToplevelsToForeProgramInfo(
        List<LswtToplevel> toplevels)
    {
        var processNameSet = new HashSet<string>();

        return (from toplevel in toplevels
            where !toplevel.Minimized
            let processName = GetProcessNameFromToplevel(toplevel)
            where processNameSet.Add(processName)
            let processPath = _processPathResolver.GetExecutablePathFromAppId(
                toplevel.AppId ?? string.Empty,
                toplevel.Title ?? string.Empty)
            select new ForeProgramInfo(
                Title: toplevel.Title ?? string.Empty,
                ProcessName: processName,
                ClassName: string.Empty, // lswt不提供类名
                ProcessFileAddress: processPath // 通过 resolver 获取实际路径
            )).ToList();
    }

    /// <summary>
    /// 从toplevel中提取进程名
    /// </summary>
    private static string GetProcessNameFromToplevel(LswtToplevel toplevel)
    {
        // 优先使用app-id作为进程名
        if (!string.IsNullOrEmpty(toplevel.AppId))
        {
            return toplevel.AppId;
        }

        // 如果app-id为空，尝试从标题中提取
        if (!string.IsNullOrEmpty(toplevel.Title))
        {
            // 简单的启发式方法：使用标题的第一部分
            var parts = toplevel.Title.Split([' ', '-', '—'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0];
            }
        }

        // 默认返回未知
        return "unknown";
    }
}

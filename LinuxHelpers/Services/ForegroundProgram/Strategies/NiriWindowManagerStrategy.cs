using System.Diagnostics;
using System.Text.Json;
using PlatformAbstractions;
using Serilog;
using LinuxHelpers.Helpers;
using LinuxHelpers.Models.Niri;

namespace LinuxHelpers.Services.ForegroundProgram.Strategies;

/// <summary>
/// Niri 窗口管理器监控策略
/// 通过 niri msg --json event-stream 监听窗口焦点变化事件
/// </summary>
public class NiriWindowManagerStrategy : IWindowManagerMonitorStrategy
{
    private const int CommandTimeoutMs = 5000;
    private const int MaxRestartAttempts = 3;

    private Process? _eventStreamProcess;
    private readonly LinuxProcessPathResolver _pathResolver = new();
    private ForeProgramInfo? _lastProgramInfo;
    private readonly Lock _lock = new();
    private int _restartCount;
    private bool _disposed;

    public event EventHandler<ForeProgramInfo>? ForeProgramChanged;
    public bool IsSupported => true;

    /// <summary>
    /// 开始监控窗口焦点变化
    /// </summary>
    public void StartMonitoring()
    {
        if (_eventStreamProcess != null)
        {
            return;
        }

        Log.Information("[{Strategy}] Starting monitoring...", nameof(NiriWindowManagerStrategy));
        StartEventStreamProcess();
    }

    /// <summary>
    /// 停止监控窗口焦点变化
    /// </summary>
    public void StopMonitoring()
    {
        Log.Information("[{Strategy}] Stopping monitoring...", nameof(NiriWindowManagerStrategy));
        CleanupEventStreamProcess();
        _restartCount = 0;
    }

    /// <summary>
    /// 启动 niri event-stream 进程
    /// </summary>
    private void StartEventStreamProcess()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "niri",
                Arguments = "msg --json event-stream",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _eventStreamProcess = Process.Start(startInfo);
            if (_eventStreamProcess == null)
            {
                Log.Error("[{Strategy}] Failed to start niri event-stream process", nameof(NiriWindowManagerStrategy));
                return;
            }

            // 监听进程退出事件
            _eventStreamProcess.Exited += OnEventStreamProcessExited;
            _eventStreamProcess.EnableRaisingEvents = true;

            // 异步读取输出
            _ = Task.Run(ReadEventStreamAsync);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Strategy}] Failed to start event-stream: {Message}", nameof(NiriWindowManagerStrategy), ex.Message);
        }
    }

    /// <summary>
    /// 异步读取事件流
    /// </summary>
    private async Task ReadEventStreamAsync()
    {
        if (_eventStreamProcess == null)
        {
            Log.Warning("[{Strategy}] Event stream process is null", nameof(NiriWindowManagerStrategy));
            return;
        }

        var stream = _eventStreamProcess.StandardOutput;

        // 异步读取错误流（避免死锁）
        _ = Task.Run(ReadErrorStreamAsync);

        try
        {
            while (true)
            {
                var line = await stream.ReadLineAsync();
                if (line == null)
                {
                    break;
                }
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                // 检查是否为焦点相关事件
                if (!line.Contains("WindowFocusChanged") &&
                    !line.Contains("WorkspaceActivated") &&
                    !line.Contains("WindowsChanged")) continue;
                await HandleWindowFocusChanged();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Strategy}] Error reading event stream: {Message}", nameof(NiriWindowManagerStrategy), ex.Message);
        }
    }

    /// <summary>
    /// 异步读取错误流
    /// </summary>
    private async Task ReadErrorStreamAsync()
    {
        if (_eventStreamProcess == null)
            return;

        try
        {
            var errorStream = _eventStreamProcess.StandardError;
            while (true)
            {
                var error = await errorStream.ReadLineAsync();
                if (error == null)
                {
                    break;
                }
                if (!string.IsNullOrEmpty(error))
                {
                    Log.Warning("[{Strategy}] Error stream: {Error}", nameof(NiriWindowManagerStrategy), error);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Strategy}] Error reading error stream: {Message}", nameof(NiriWindowManagerStrategy), ex.Message);
        }
    }

    /// <summary>
    /// 处理窗口焦点变化事件
    /// </summary>
    private async Task HandleWindowFocusChanged()
    {
        try
        {
            // 获取当前焦点窗口信息
            var focusedWindowInfo = await GetFocusedWindowInfoAsync();
            if (focusedWindowInfo == null)
            {
                Log.Warning("[{Strategy}] Failed to get focused window info", nameof(NiriWindowManagerStrategy));
                return;
            }

            // 创建 ForeProgramInfo
            var programInfo = CreateForeProgramInfo(focusedWindowInfo);

            // 去重检查
            lock (_lock)
            {
                if (_lastProgramInfo != null &&
                    _lastProgramInfo.ProcessName == programInfo.ProcessName &&
                    _lastProgramInfo.Title == programInfo.Title)
                {
                    return;
                }

                _lastProgramInfo = programInfo;
            }

            // 触发事件
            ForeProgramChanged?.Invoke(this, programInfo);
            Log.Information("[{Strategy}] Window changed: [{Process}] {Title}",
                nameof(NiriWindowManagerStrategy), programInfo.ProcessName, programInfo.Title);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Strategy}] HandleWindowFocusChanged failed: {Message}", nameof(NiriWindowManagerStrategy), ex.Message);
        }
    }

    /// <summary>
    /// 获取当前焦点窗口信息
    /// </summary>
    private async Task<NiriFocusedWindowResponse?> GetFocusedWindowInfoAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "niri",
                Arguments = "msg --json focused-window",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Log.Error("[{Strategy}] Failed to start focused-window command", nameof(NiriWindowManagerStrategy));
                return null;
            }

            var json = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(CommandTimeoutMs))
            {
                Log.Warning("[{Strategy}] focused-window command timeout", nameof(NiriWindowManagerStrategy));
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Ignore
                }
                return null;
            }

            if (process.ExitCode != 0)
            {
                Log.Error("[{Strategy}] focused-window command failed (exit code: {ExitCode}): {Error}",
                    nameof(NiriWindowManagerStrategy), process.ExitCode, error);
                return null;
            }

            var response = JsonSerializer.Deserialize(
                json,
                NiriJsonSgContext.Default.NiriFocusedWindowResponse);

            if (response == null)
            {
                Log.Warning("[{Strategy}] Failed to deserialize focused window response", nameof(NiriWindowManagerStrategy));
                return null;
            }

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Strategy}] GetFocusedWindowInfoAsync failed: {Message}", nameof(NiriWindowManagerStrategy), ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 创建 ForeProgramInfo
    /// </summary>
    private ForeProgramInfo CreateForeProgramInfo(NiriFocusedWindowResponse window)
    {
        var processName = !string.IsNullOrEmpty(window.AppId)
            ? window.AppId
            : "unknown";

        var processPath = _pathResolver.GetExecutablePathFromAppId(
            window.AppId ?? string.Empty,
            window.Title ?? string.Empty);

        return new ForeProgramInfo(
            Title: window.Title ?? string.Empty,
            ProcessName: processName,
            ClassName: string.Empty, // Niri 不提供类名
            ProcessFileAddress: processPath
        );
    }

    /// <summary>
    /// 事件流进程退出处理
    /// </summary>
    private void OnEventStreamProcessExited(object? sender, EventArgs e)
    {
        if (_disposed)
        {
            return;
        }

        // 尝试重启
        if (_restartCount < MaxRestartAttempts)
        {
            _restartCount++;

            CleanupEventStreamProcess();

            // 延迟一段时间后再重启
            Task.Delay(1000).ContinueWith(_ =>
            {
                if (!_disposed)
                {
                    StartEventStreamProcess();
                }
            });
        }
        else
        {
            Log.Warning("[{Strategy}] Max restart attempts reached, stopping monitoring", nameof(NiriWindowManagerStrategy));
            CleanupEventStreamProcess();
        }
    }

    /// <summary>
    /// 清理事件流进程
    /// </summary>
    private void CleanupEventStreamProcess()
    {
        try
        {
            if (_eventStreamProcess != null)
            {
                if (!_eventStreamProcess.HasExited)
                {
                    _eventStreamProcess.Kill(entireProcessTree: true);
                }

                _eventStreamProcess.Exited -= OnEventStreamProcessExited;
                _eventStreamProcess.Dispose();
                _eventStreamProcess = null;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{Strategy}] Error cleaning up event stream process: {Message}", nameof(NiriWindowManagerStrategy), ex.Message);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopMonitoring();
        GC.SuppressFinalize(this);
    }
}

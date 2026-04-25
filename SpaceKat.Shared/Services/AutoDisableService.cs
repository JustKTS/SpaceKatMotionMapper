using Avalonia.Threading;
using PlatformAbstractions;
using Serilog;
using SpaceKat.Shared.Services.Contract;

namespace SpaceKat.Shared.Services;

public class AutoDisableService
{
    /// <summary>
    /// 自动禁用程序信息
    /// </summary>
    private class AutoDisableProgramInfo
    {
        public string Path { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// 检查是否匹配前台程序信息
        /// </summary>
        public bool Matches(ForeProgramInfo info)
        {
            // 优先路径匹配
            if (!string.IsNullOrEmpty(info.ProcessFileAddress) &&
                !string.IsNullOrEmpty(Path) &&
                Path == info.ProcessFileAddress)
            {
                return true;
            }

            // 降级到进程名匹配
            if (!string.IsNullOrEmpty(info.ProcessName) &&
                !string.IsNullOrEmpty(ProcessName) &&
                ProcessName == info.ProcessName)
            {
                return true;
            }

            return false;
        }
    }

    private readonly List<AutoDisableProgramInfo> _autoDisablePrograms = [];

    private readonly IPlatformForegroundProgramService _currentForeProgramHelper;
    private readonly ILocalSettingsService _localSettingsService;
    private const string AutoDisableProgramsKey = "AutoDisablePrograms";
    private const string AutoDisableKey = "IsAutoDisable";

    private bool _isEnable;

    public bool IsInitialized { get; private set; }
    public bool IsPlatformSupported => _currentForeProgramHelper.IsSupported;
    public event EventHandler<bool>? IsCurrentFpInList;

    public bool IsEnable
    {
        get => _isEnable;
        set
        {
            // 强制检查：平台不支持时，绝对不允许启用
            if (value && !_currentForeProgramHelper.IsSupported)
            {
                Log.Warning("[{Service}] Cannot enable: platform not supported", nameof(AutoDisableService));
                return; // 阻止启用
            }

            if (value)
            {
                Log.Information("[{Service}] Auto-disable enabled", nameof(AutoDisableService));
                _localSettingsService.SaveSettingAsync(AutoDisableKey, value).ContinueWith(t =>
                {
                    if (t.IsFaulted) Log.Error(t.Exception, "[{Service}] Failed to save IsEnable setting", nameof(AutoDisableService));
                });
                _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangedHandle;
            }
            else
            {
                Log.Information("[{Service}] Auto-disable disabled", nameof(AutoDisableService));
                _currentForeProgramHelper.ForeProgramChanged -= ForeProgramChangedHandle;
            }

            _isEnable = value;
        }
    }

    public AutoDisableService(IPlatformForegroundProgramService currentForeProgramHelper,
        ILocalSettingsService localSettingsService)
    {
        _currentForeProgramHelper = currentForeProgramHelper;
        _localSettingsService = localSettingsService;
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        // 检查平台支持并记录日志
        if (!_currentForeProgramHelper.IsSupported)
        {
            Log.Warning("[{Service}] Platform does NOT support foreground program monitoring", nameof(AutoDisableService));
            Log.Warning("[{Service}] Auto-disable feature will be disabled", nameof(AutoDisableService));
        }
        else
        {
            Log.Information("[{Service}] Platform supports foreground program monitoring", nameof(AutoDisableService));
        }

        var keys = await _localSettingsService.ReadSettingAsync<List<string>>(AutoDisableProgramsKey);
        if (keys is not null)
        {
            // 向后兼容：旧数据只有路径，没有进程名
            _autoDisablePrograms.AddRange(keys.Select(path => new AutoDisableProgramInfo
            {
                Path = path,
                ProcessName = string.Empty // 旧数据没有进程名
            }));
            Log.Information("[{Service}] Loaded {Count} programs from settings", nameof(AutoDisableService), _autoDisablePrograms.Count);
        }

        // 只有在平台支持时才加载启用状态
        var isEnable = await _localSettingsService.ReadSettingAsync<bool?>(AutoDisableKey);
        if (isEnable is not null)
        {
            // 强制检查：如果平台不支持，即使设置中保存了启用状态，也强制禁用
            if (!IsPlatformSupported)
            {
                Log.Warning("[{Service}] Platform not supported, forcing IsEnable to false", nameof(AutoDisableService));
                await Dispatcher.UIThread.InvokeAsync(() => { IsEnable = false; });
                // 清除保存的启用状态
                await _localSettingsService.SaveSettingAsync(AutoDisableKey, false);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() => { IsEnable = isEnable.Value; });
            }
        }

        IsInitialized = true;
    }

    public async Task<bool> WaitForInitializedAsync()
    {
        var count = 0;
        while (!IsInitialized || count == 20) 
        {
            await Task.Delay(100);
            count++;
        }

        return IsInitialized;
    }

    public void AddProgramPath(string programPath, string processName = "")
    {
        // 检查是否已存在相同路径
        if (_autoDisablePrograms.Any(x => x.Path == programPath))
            return;

        var info = new AutoDisableProgramInfo
        {
            Path = programPath,
            ProcessName = processName
        };

        _autoDisablePrograms.Add(info);
        Log.Debug("[{Service}] Added program: Path={Path}, ProcessName={ProcessName}",
            nameof(AutoDisableService), programPath, processName);
        SavePrograms();
    }

    public void RemoveProgramPath(string programPath)
    {
        var info = _autoDisablePrograms.FirstOrDefault(x => x.Path == programPath);
        if (info == null) return;

        _autoDisablePrograms.Remove(info);
        Log.Debug("[{Service}] Removed program: {Path}", nameof(AutoDisableService), programPath);
        SavePrograms();
    }

    public bool IsPathContained(string path)
    {
        return _autoDisablePrograms.Any(x => x.Path == path);
    }

    public string[] GetAllProgramPaths()
    {
        return _autoDisablePrograms.Select(x => x.Path).ToArray();
    }

    private void ForeProgramChangedHandle(object? obj, ForeProgramInfo info)
    {
        Log.Debug("[{Service}] ForeProgram changed: ProcessName={ProcessName}, Path={Path}, Title={Title}",
            nameof(AutoDisableService), info.ProcessName, info.ProcessFileAddress, info.Title);

        // 使用新的匹配逻辑
        var isContained = _autoDisablePrograms.Any(programInfo =>
            programInfo.Matches(info));

        Log.Information("[{Service}] Is in disable list: {IsContained}, Mapper enabled: {MapperEnabled}",
            nameof(AutoDisableService), isContained, !isContained);

        IsCurrentFpInList?.Invoke(this, !isContained);
    }

    /// <summary>
    /// 保存程序列表到设置
    /// </summary>
    private void SavePrograms()
    {
        var paths = _autoDisablePrograms.Select(x => x.Path).ToList();
        _localSettingsService.SaveSettingAsync(AutoDisableProgramsKey, paths).ContinueWith(t =>
        {
            if (t.IsFaulted) Log.Error(t.Exception, "[{Service}] Failed to save auto-disable programs", nameof(AutoDisableService));
        });
    }
}
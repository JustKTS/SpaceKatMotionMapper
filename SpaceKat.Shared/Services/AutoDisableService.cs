using Avalonia.Threading;
using SpaceKat.Shared.Services.Contract;
using Win32Helpers;

namespace SpaceKat.Shared.Services;

public class AutoDisableService
{
    private readonly List<string> _autoDisablePrograms = [];

    private readonly CurrentForeProgramHelper _currentForeProgramHelper;
    private readonly ILocalSettingsService _localSettingsService;
    private const string AutoDisableProgramsKey = "AutoDisablePrograms";
    private const string AutoDisableKey = "IsAutoDisable";

    private bool _isEnable;

    public bool IsInitialized { get; private set; }
    public event EventHandler<bool>? IsCurrentFpInList;

    public bool IsEnable
    {
        get => _isEnable;
        set
        {
            if (value)
            {
                _localSettingsService.SaveSettingAsync(AutoDisableKey, value).Wait();
                _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangedHandle;
            }
            else
            {
                _currentForeProgramHelper.ForeProgramChanged -= ForeProgramChangedHandle;
            }

            _isEnable = value;
        }
    }

    public AutoDisableService(CurrentForeProgramHelper currentForeProgramHelper,
        ILocalSettingsService localSettingsService)
    {
        _currentForeProgramHelper = currentForeProgramHelper;
        _localSettingsService = localSettingsService;
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;
        var keys = await _localSettingsService.ReadSettingAsync<List<string>>(AutoDisableProgramsKey);
        if (keys is not null)
        {
            _autoDisablePrograms.AddRange(keys);
        }
        var isEnable = await _localSettingsService.ReadSettingAsync<bool?>(AutoDisableKey);
        if (isEnable is not null)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { IsEnable = isEnable.Value; });
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

    public void AddProgramPath(string programPath)
    {
        if (_autoDisablePrograms.Contains(programPath)) return;
        _autoDisablePrograms.Add(programPath);
        _localSettingsService.SaveSettingAsync(AutoDisableProgramsKey, _autoDisablePrograms).Wait();
    }

    public void RemoveProgramPath(string programPath)
    {
        if (!_autoDisablePrograms.Contains(programPath)) return;
        _autoDisablePrograms.Remove(programPath);
        _localSettingsService.SaveSettingAsync(AutoDisableProgramsKey, _autoDisablePrograms).Wait();
    }

    public bool IsPathContained(string path)
    {
        return _autoDisablePrograms.Contains(path);
    }

    public string[] GetAllProgramPaths()
    {
        return _autoDisablePrograms.ToArray();
    }

    private void ForeProgramChangedHandle(object? obj, ForeProgramInfo info)
    {
        if (string.IsNullOrEmpty(info.ProcessFileAddress)) return;
        IsCurrentFpInList?.Invoke(this,  !_autoDisablePrograms.Contains(info.ProcessFileAddress));
    }
}
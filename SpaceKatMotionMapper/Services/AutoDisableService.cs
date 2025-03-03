﻿using System.Collections.Generic;
using SpaceKatHIDWrapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using Win32Helpers;

namespace SpaceKatMotionMapper.Services;

public class AutoDisableService
{
    private readonly List<string> _autoDisablePrograms = [];

    private readonly CurrentForeProgramHelper _currentForeProgramHelper;
    private readonly ILocalSettingsService _localSettingsService;
    private const string AutoDisableProgramsKey = "AutoDisablePrograms";
    private const string AutoDisableKey = "IsAutoDisable";

    private bool _isEnable;

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
                _localSettingsService.SaveSettingAsync(AutoDisableKey, value).Wait();
            }

            _isEnable = value;
        }
    }

    public AutoDisableService(CurrentForeProgramHelper currentForeProgramHelper,
        ILocalSettingsService localSettingsService)
    {
        _currentForeProgramHelper = currentForeProgramHelper;
        _localSettingsService = localSettingsService;
        _localSettingsService.ReadSettingAsync<List<string>>(AutoDisableProgramsKey).ContinueWith(t =>
        {
            if (t.Result is null) return;
            _autoDisablePrograms.AddRange(t.Result);
        });
        _localSettingsService.ReadSettingAsync<bool?>(AutoDisableKey).ContinueWith(t =>
        {
            IsEnable = t.Result ?? false;
        });
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
        var vm = App.GetRequiredService<ListeningInfoViewModel>();
        vm.IsOfficialMapperOff = !_autoDisablePrograms.Contains(info.ProcessFileAddress);
    }
}
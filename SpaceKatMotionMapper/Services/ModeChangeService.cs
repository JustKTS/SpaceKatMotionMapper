using System;
using System.Collections.Generic;
using Avalonia.Threading;
using Serilog;
using SpaceKatMotionMapper.Models;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.Services;

public class ModeChangeService
{
    public int CurrentMode { get; set; }
    public bool ConfigIsDefault { get; private set; } = true;

    public Guid CurrentActivatedConfig { get; private set; } = Guid.Empty;
    public bool IsPlatformSupported => _currentForeProgramHelper.IsSupported;

    private readonly IPlatformForegroundProgramService _currentForeProgramHelper;
    private readonly KatMotionTimeConfigService _katMotionTimeConfigService;
    private readonly KatDeadZoneConfigService _katDeadZoneConfigService;
    private readonly KatMotionConfigVMManageService _katMotionConfigVmManageService;
    public ForeProgramInfo? CurrentForeProgramInfo { get; private set; }
    private Dictionary<string, Guid> BindProcessPathList { get; } = [];

    public ModeChangeService(IPlatformForegroundProgramService currentForeProgramHelper,
        KatMotionTimeConfigService katMotionTimeConfigService,
        KatDeadZoneConfigService katDeadZoneConfigService,
        KatMotionConfigVMManageService katMotionConfigVmManageService
    )
    {
        _katMotionConfigVmManageService = katMotionConfigVmManageService;
        _katMotionTimeConfigService = katMotionTimeConfigService;
        _katDeadZoneConfigService = katDeadZoneConfigService;
        _currentForeProgramHelper = currentForeProgramHelper;

        // 添加平台支持日志
        if (_currentForeProgramHelper.IsSupported)
        {
            Log.Information("[{Service}] Platform supports foreground program monitoring", nameof(ModeChangeService));
        }
        else
        {
            Log.Warning("[{Service}] Platform does NOT support foreground program monitoring", nameof(ModeChangeService));
            Log.Warning("[{Service}] Window mode switching will be disabled", nameof(ModeChangeService));
        }

        _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangeHandler;
    }

    private void ForeProgramChangeHandler(object? sender, ForeProgramInfo data)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentForeProgramInfo = data;
            var ret = BindProcessPathList.TryGetValue(CurrentForeProgramInfo.ProcessFileAddress, out var id);
            ConfigIsDefault = string.IsNullOrEmpty(CurrentForeProgramInfo.ProcessFileAddress) || !ret;

            if (ConfigIsDefault)
            {
                CurrentActivatedConfig = Guid.Empty;
                _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
            }
            else
            {
                CurrentActivatedConfig = id;
                _katMotionConfigVmManageService.GetConfig(id)
                    .Iter(configVm =>
                    {
                        if (configVm.IsCustomMotionTimeConfigs)
                            _katMotionTimeConfigService.ApplyMotionTimeConfigById(id);
                        else _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                        if (configVm.IsCustomDeadZone) _katDeadZoneConfigService.ApplyDeadZoneConfigById(id);
                        else _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
                    });
            }
        });
    }

    public void UpdateBindProcessPathList(KatMotionConfigGroup configGroup)
    {
        BindProcessPathList[configGroup.ProcessPath] = Guid.Parse(configGroup.Guid);
    }

    public void RemovePathForBindProcessPathList(string processPath)
    {
        BindProcessPathList.Remove(processPath);
    }
}
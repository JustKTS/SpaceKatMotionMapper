using System;
using System.Collections.Generic;
using Avalonia.Threading;
using Serilog;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.Services;

public class ModeChangeService : IModeChangeService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ModeChangeService>();

    public int CurrentMode { get; set; }
    public bool ConfigIsDefault { get; private set; } = true;

    public Guid CurrentActivatedConfig { get; private set; } = Guid.Empty;
    public bool IsPlatformSupported => _currentForeProgramHelper.IsSupported;

    private readonly IPlatformForegroundProgramService _currentForeProgramHelper;
    private readonly IKatMotionTimeConfigService _katMotionTimeConfigService;
    private readonly IKatDeadZoneConfigService _katDeadZoneConfigService;
    private readonly IKatMotionConfigVMManageService _katMotionConfigVmManageService;
    public ForeProgramInfo? CurrentForeProgramInfo { get; private set; }
    private Dictionary<string, Guid> BindProcessPathList { get; } = [];

    public ModeChangeService(IPlatformForegroundProgramService currentForeProgramHelper,
        IKatMotionTimeConfigService katMotionTimeConfigService,
        IKatDeadZoneConfigService katDeadZoneConfigService,
        IKatMotionConfigVMManageService katMotionConfigVmManageService
    )
    {
        _katMotionConfigVmManageService = katMotionConfigVmManageService;
        _katMotionTimeConfigService = katMotionTimeConfigService;
        _katDeadZoneConfigService = katDeadZoneConfigService;
        _currentForeProgramHelper = currentForeProgramHelper;

        // 添加平台支持日志
        if (_currentForeProgramHelper.IsSupported)
        {
            Log.Information("Platform supports foreground program monitoring");
        }
        else
        {
            Log.Warning("Platform does NOT support foreground program monitoring. Window mode switching will be disabled");
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
                var configVmResult = _katMotionConfigVmManageService.GetConfig(id);
                if (configVmResult.IsSuccess)
                {
                    var configVm = configVmResult.Value;
                    if (configVm.IsCustomMotionTimeConfigs)
                        _katMotionTimeConfigService.ApplyMotionTimeConfigById(id);
                    else _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                    if (configVm.IsCustomDeadZone) _katDeadZoneConfigService.ApplyDeadZoneConfigById(id);
                    else _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
                }
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
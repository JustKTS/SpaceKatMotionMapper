using System;
using System.Collections.Generic;
using Avalonia.Threading;
using SpaceKatMotionMapper.Models;
using Win32Helpers;

namespace SpaceKatMotionMapper.Services;

public class ModeChangeService
{
    public int CurrentMode { get; set; }
    public bool ConfigIsDefault { get; private set; } = true;

    public Guid CurrentActivatedConfig { get; private set; } = Guid.Empty;

    private readonly CurrentForeProgramHelper _currentForeProgramHelper;
    private readonly KatMotionTimeConfigService _katMotionTimeConfigService;
    private readonly KatDeadZoneConfigService _katDeadZoneConfigService;
    private readonly KatMotionConfigVMManageService _katMotionConfigVmManageService;
    public ForeProgramInfo? CurrentForeProgramInfo { get; private set; }
    private Dictionary<string, Guid> BindProcessPathList { get; } = [];

    public ModeChangeService(CurrentForeProgramHelper currentForeProgramHelper,
        KatMotionTimeConfigService katMotionTimeConfigService,
        KatDeadZoneConfigService katDeadZoneConfigService,
        KatMotionConfigVMManageService katMotionConfigVmManageService
    )
    {
        _katMotionConfigVmManageService = katMotionConfigVmManageService;
        _katMotionTimeConfigService = katMotionTimeConfigService;
        _katDeadZoneConfigService = katDeadZoneConfigService;
        _currentForeProgramHelper = currentForeProgramHelper;
        _currentForeProgramHelper.ForeProgramChanged += ForeProgramChangeHandler;
    }

    private void ForeProgramChangeHandler(object? sender, ForeProgramInfo data)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentForeProgramInfo = data;
            var ret = BindProcessPathList.TryGetValue(CurrentForeProgramInfo.ProcessFileAddress, out var id);
            ConfigIsDefault = string.IsNullOrEmpty(data.ProcessFileAddress) || !ret;

            if (ConfigIsDefault)
            {
                CurrentActivatedConfig = Guid.Empty;
                _katMotionTimeConfigService.ApplyDefaultMotionTimeConfig();
                _katDeadZoneConfigService.ApplyDefaultDeadZoneConfig();
            }
            else
            {
                CurrentActivatedConfig = id;
                var configRet = _katMotionConfigVmManageService.GetConfig(id);
                configRet.IfSucc(configVm =>
                {
                    if (configVm.IsCustomMotionTimeConfigs) _katMotionTimeConfigService.ApplyMotionTimeConfigById(id);
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
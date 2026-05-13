using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Serilog;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class KatMotionConfigVMManageService : IKatMotionConfigVMManageService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<KatMotionConfigVMManageService>();

    private readonly Dictionary<Guid, KatMotionConfigViewModel> _configs = [];

    private Guid _commonConfigGuid = Guid.Empty;
    public void RegisterConfig(KatMotionConfigViewModel configVm)
    {
        _configs[configVm.Id] = configVm;
    }

    public Result<KatMotionConfigViewModel, Exception> GetConfig(Guid id)
    {
        try
        {
            return _configs[id];
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get config by ID: {Id}", id);
            return new Exception("获取配置组失败");
        }
    }

    public void RegisterDefaultConfig(KatMotionConfigViewModel configVm)
    {
        _commonConfigGuid = configVm.Id;
        _configs[configVm.Id] = configVm;
    }

    public Result<KatMotionConfigViewModel, Exception> GetDefaultConfig()
    {
        return _commonConfigGuid == Guid.Empty
            ? new Exception("全局配置未设置")
            : _configs[_commonConfigGuid];
    }

    public bool RemoveConfig(Guid id)
    {
        return _configs.Remove(id);
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Automation;
using LanguageExt.Common;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Services;

// ReSharper disable once InconsistentNaming
public class KatMotionConfigVMManageService
{
    private readonly Dictionary<Guid, KatMotionConfigViewModel> _configs = [];

    private Guid _commonConfigGuid = Guid.Empty;
    public void RegisterConfig(KatMotionConfigViewModel configVm)
    {
        _configs[configVm.Id] = configVm;
    }
    

    public Result<KatMotionConfigViewModel> GetConfig(Guid id)
    {
        try
        {
            return _configs[id];
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new Result<KatMotionConfigViewModel>(new Exception("获取配置组失败"));
        }
    }
    
    public void RegisterDefaultConfig(KatMotionConfigViewModel configVm)
    {
        _commonConfigGuid = configVm.Id;
        _configs[configVm.Id] = configVm;
    }
    
    public  Result<KatMotionConfigViewModel> GetDefaultConfig()
    {
        return _commonConfigGuid == Guid.Empty 
            ? new Result<KatMotionConfigViewModel>(new Exception("全局配置未设置")) 
            : _configs[_commonConfigGuid];
    }


    public bool RemoveConfig(Guid id)
    {
        return _configs.Remove(id);
    }
}
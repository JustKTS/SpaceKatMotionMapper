using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Automation;
using LanguageExt.Common;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Services;

// ReSharper disable once InconsistentNaming
public class KatActionConfigVMManageService
{
    private readonly Dictionary<Guid, KatActionConfigViewModel> _configs = [];

    private Guid _commonConfigGuid = Guid.Empty;
    public void RegisterConfig(KatActionConfigViewModel configVm)
    {
        _configs[configVm.Id] = configVm;
    }
    

    public Result<KatActionConfigViewModel> GetConfig(Guid id)
    {
        try
        {
            return _configs[id];
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new Result<KatActionConfigViewModel>(new Exception("获取配置组失败"));
        }
    }
    
    public void RegisterDefaultConfig(KatActionConfigViewModel configVm)
    {
        _commonConfigGuid = configVm.Id;
        _configs[configVm.Id] = configVm;
    }
    
    public  Result<KatActionConfigViewModel> GetDefaultConfig()
    {
        return _commonConfigGuid == Guid.Empty 
            ? new Result<KatActionConfigViewModel>(new Exception("全局配置未设置")) 
            : _configs[_commonConfigGuid];
    }


    public bool RemoveConfig(Guid id)
    {
        return _configs.Remove(id);
    }
}
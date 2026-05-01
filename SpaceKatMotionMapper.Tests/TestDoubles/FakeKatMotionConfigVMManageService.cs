using CSharpFunctionalExtensions;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

public class FakeKatMotionConfigVMManageService : IKatMotionConfigVMManageService
{
    private readonly Dictionary<Guid, KatMotionConfigViewModel> _configs = [];
    private Guid _commonConfigGuid = Guid.Empty;

    public void RegisterConfig(KatMotionConfigViewModel configVm)
    {
        _configs[configVm.Id] = configVm;
    }

    public void RegisterDefaultConfig(KatMotionConfigViewModel configVm)
    {
        _commonConfigGuid = configVm.Id;
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
            return new Exception("获取配置组失败", e);
        }
    }

    public Result<KatMotionConfigViewModel, Exception> GetDefaultConfig()
    {
        if (_commonConfigGuid != Guid.Empty)
        {
            return _configs[_commonConfigGuid];
        }
        return new Exception("测试环境中没有默认配置");
    }

    public bool RemoveConfig(Guid id)
    {
        return _configs.Remove(id);
    }
}


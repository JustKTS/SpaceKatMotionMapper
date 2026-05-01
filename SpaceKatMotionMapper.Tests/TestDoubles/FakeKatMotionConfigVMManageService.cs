using LanguageExt;
using Moq;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.ViewModels;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 用于测试的配置管理服务 Fake 实现
/// </summary>
public class FakeKatMotionConfigVMManageService : IKatMotionConfigVMManageService
{
    private readonly Dictionary<Guid, KatMotionConfigViewModel> _configs = [];
    private Guid _commonConfigGuid = Guid.Empty;
    /// <inheritdoc />
    public void RegisterConfig(KatMotionConfigViewModel configVm)
    {
        _configs[configVm.Id] = configVm;
    }

    /// <inheritdoc />
    public void RegisterDefaultConfig(KatMotionConfigViewModel configVm)
    {
        _commonConfigGuid = configVm.Id;
        _configs[configVm.Id] = configVm;
    }

    /// <inheritdoc />
    public Either<Exception, KatMotionConfigViewModel> GetConfig(Guid id)
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

    /// <inheritdoc />
    public Either<Exception, KatMotionConfigViewModel> GetDefaultConfig()
    {
        if (_commonConfigGuid != Guid.Empty)
        {
            return _configs[_commonConfigGuid];
        }

        // 返回错误而不是创建默认配置，避免循环依赖
        return new Exception("测试环境中没有默认配置");
    }

    /// <inheritdoc />
    public bool RemoveConfig(Guid id)
    {
        return _configs.Remove(id);
    }
}


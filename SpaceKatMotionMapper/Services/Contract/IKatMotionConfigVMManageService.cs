using System;
using LanguageExt;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Services.Contract;

/// <summary>
/// 管理 KatMotionConfigViewModel 实例的生命周期
/// </summary>
public interface IKatMotionConfigVMManageService
{
    /// <summary>
    /// 注册配置实例
    /// </summary>
    /// <param name="configVm">配置ViewModel</param>
    void RegisterConfig(KatMotionConfigViewModel configVm);

    /// <summary>
    /// 注册默认配置实例
    /// </summary>
    /// <param name="configVm">配置ViewModel</param>
    void RegisterDefaultConfig(KatMotionConfigViewModel configVm);

    /// <summary>
    /// 根据ID获取配置实例
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>配置实例或异常</returns>
    Either<Exception, KatMotionConfigViewModel> GetConfig(Guid id);

    /// <summary>
    /// 获取默认配置实例
    /// </summary>
    /// <returns>默认配置实例或异常</returns>
    Either<Exception, KatMotionConfigViewModel> GetDefaultConfig();

    /// <summary>
    /// 移除配置实例
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>是否成功移除</returns>
    bool RemoveConfig(Guid id);
}

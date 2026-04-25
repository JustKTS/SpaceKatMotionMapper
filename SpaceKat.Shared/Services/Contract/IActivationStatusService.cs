namespace SpaceKat.Shared.Services.Contract;

/// <summary>
/// 管理 KatMotion 配置的激活状态
/// </summary>
public interface IActivationStatusService
{
    /// <summary>
    /// 设置配置组的激活状态
    /// </summary>
    /// <param name="configGroupId">配置组ID</param>
    /// <param name="isActivated">是否激活</param>
    void SetActivationStatus(Guid configGroupId, bool isActivated);

    /// <summary>
    /// 检查配置组是否已激活
    /// </summary>
    /// <param name="configGroupId">配置组ID</param>
    /// <returns>是否已激活</returns>
    bool IsConfigGroupActivated(Guid configGroupId);

    /// <summary>
    /// 删除配置组的激活状态
    /// </summary>
    /// <param name="configGroupId">配置组ID</param>
    void DeleteActivationStatus(Guid configGroupId);

    /// <summary>
    /// 保存激活状态到持久化存储
    /// </summary>
    void SaveActivationStatus();
}

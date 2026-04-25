using SpaceKat.Shared.Services.Contract;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 用于测试的激活状态服务 Fake 实现
/// </summary>
public class FakeActivationStatusService : IActivationStatusService
{
    private readonly Dictionary<Guid, bool> _activationStatus = [];

    /// <inheritdoc />
    public void SetActivationStatus(Guid configGroupId, bool isActivated)
    {
        _activationStatus[configGroupId] = isActivated;
    }

    /// <inheritdoc />
    public bool IsConfigGroupActivated(Guid configGroupId)
    {
        return _activationStatus.ContainsKey(configGroupId) && _activationStatus[configGroupId];
    }

    /// <inheritdoc />
    public void DeleteActivationStatus(Guid configGroupId)
    {
        _activationStatus.Remove(configGroupId);
    }

    /// <inheritdoc />
    public void SaveActivationStatus()
    {
        // 测试中不需要实际保存
    }
}

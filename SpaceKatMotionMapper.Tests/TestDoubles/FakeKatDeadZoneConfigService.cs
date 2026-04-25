using Moq;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatHIDWrapper.Services;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的KatDeadZoneConfigService简单实现
/// </summary>
public class FakeKatDeadZoneConfigService : KatDeadZoneConfigService
{
    public FakeKatDeadZoneConfigService()
        : base(
              Mock.Of<IKatMotionFileService>(),
              new FakeKatMotionRecognizeService(),
              Mock.Of<IKatMotionConfigVMManageService>()
          )
    {
    }

    /// <summary>
    /// 重写以避免循环依赖
    /// </summary>
    public new KatDeadZoneConfig LoadDefaultDeadZoneConfigs()
    {
        // 返回一个简单的默认配置，避免调用 GetDefaultConfig() 导致的循环依赖
        return new KatDeadZoneConfig
        {
            Upper = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1],
            Lower = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1],
            AxesInverse = [false, false, false, false, false, false]
        };
    }
}

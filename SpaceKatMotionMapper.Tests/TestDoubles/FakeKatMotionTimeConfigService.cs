using Moq;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatHIDWrapper.Services;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的KatMotionTimeConfigService简单实现
/// </summary>
public class FakeKatMotionTimeConfigService : KatMotionTimeConfigService
{
    public FakeKatMotionTimeConfigService()
        : base(
              Mock.Of<IKatMotionFileService>(),
              Mock.Of<IKatMotionConfigVMManageService>(),
              new FakeKatMotionRecognizeService()
          )
    {
    }
}

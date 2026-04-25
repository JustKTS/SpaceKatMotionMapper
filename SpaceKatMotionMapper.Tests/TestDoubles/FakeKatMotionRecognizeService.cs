using Moq;
using SpaceKatHIDWrapper.Services;
using SpaceKatHIDWrapper.DeviceWrappers;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的KatMotionRecognizeService简单实现
/// </summary>
public class FakeKatMotionRecognizeService : KatMotionRecognizeService
{
    public FakeKatMotionRecognizeService()
        : base(Mock.Of<IDeviceDataWrapper>())
    {
    }
}

using Moq;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.States;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的TimeAndDeadZoneVMService简单实现
/// </summary>
public class FakeTimeAndDeadZoneVMService : TimeAndDeadZoneVMService
{
    public FakeTimeAndDeadZoneVMService()
        : base(
              CreateFakeMotionTimeConfigViewModel(),
              CreateFakeDeadZoneConfigViewModel(),
              new FakeTimeAndDeadZoneSettingViewModel(),
              new GlobalStates()
          )
    {
    }

    private static FakeMotionTimeConfigViewModel CreateFakeMotionTimeConfigViewModel()
    {
        return new FakeMotionTimeConfigViewModel();
    }

    private static FakeDeadZoneConfigViewModel CreateFakeDeadZoneConfigViewModel()
    {
        return new FakeDeadZoneConfigViewModel();
    }
}

using Moq;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的TimeAndDeadZoneSettingViewModel简单实现
/// 避免构造时的依赖问题
/// </summary>
public class FakeTimeAndDeadZoneSettingViewModel : TimeAndDeadZoneSettingViewModel
{
    public FakeTimeAndDeadZoneSettingViewModel()
        : base(
              new FakeKatMotionRecognizeService(),
              Mock.Of<KatMotionConfigVMManageService>(),
              Mock.Of<PopUpNotificationService>()
          )
    {
    }
}

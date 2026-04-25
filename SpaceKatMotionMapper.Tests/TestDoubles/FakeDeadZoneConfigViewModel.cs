using Moq;
using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.ViewModels;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的DeadZoneConfigViewModel简单实现
/// 避免构造时的依赖问题
/// </summary>
public class FakeDeadZoneConfigViewModel : DeadZoneConfigViewModel
{
    public FakeDeadZoneConfigViewModel()
        : base(
              new FakeKatMotionRecognizeService(),
              new NoInitKatDeadZoneConfigService()
          )
    {
    }

    /// <summary>
    /// 专用于测试的假死区配置服务，不初始化死区配置以避免循环依赖
    /// </summary>
    private class NoInitKatDeadZoneConfigService : KatDeadZoneConfigService
    {
        public NoInitKatDeadZoneConfigService()
            : base(
                  Mock.Of<IKatMotionFileService>(),
                  new FakeKatMotionRecognizeService(),
                  Mock.Of<IKatMotionConfigVMManageService>()
              )
        {
        }

        // 重写以返回默认配置，不调用 GetDefaultConfig
        public new KatDeadZoneConfig LoadDefaultDeadZoneConfigs()
        {
            return new KatDeadZoneConfig
            {
                Upper = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1],
                Lower = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1],
                AxesInverse = [false, false, false, false, false, false]
            };
        }
    }
}

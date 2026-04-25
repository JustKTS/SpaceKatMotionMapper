using SpaceKatMotionMapper.Services;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的MotionTimeConfigViewModel简单实现
/// 避免构造时的依赖问题
/// </summary>
public class FakeMotionTimeConfigViewModel : MotionTimeConfigViewModel
{
    public FakeMotionTimeConfigViewModel()
        : base(
              new FakeKatMotionRecognizeService(),
              new FakeKatMotionTimeConfigService()
          )
    {
    }
}

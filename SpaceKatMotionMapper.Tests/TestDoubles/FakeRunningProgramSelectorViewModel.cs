using Moq;
using PlatformAbstractions;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.Services;

namespace SpaceKatMotionMapper.Tests.TestDoubles;

/// <summary>
/// 测试用的 RunningProgramSelectorViewModel 简单实现
/// </summary>
public class FakeRunningProgramSelectorViewModel : SpaceKat.Shared.ViewModels.RunningProgramSelectorViewModel
{
    public FakeRunningProgramSelectorViewModel()
        : base(
            Mock.Of<IStorageProviderService>(),
            Mock.Of<IPlatformWindowService>()
        )
    {
    }
}

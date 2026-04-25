using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKatMotionActivateService
{
    bool IsActivated { get; set; }
    void ActivateKatMotions(KatMotionConfigGroup configGroup);
    void DeactivateKatMotions(KatMotionConfigGroup configGroup);
}

using System;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface ITransparentInfoActionDisplayService
{
    void Register(Guid motionId, Guid displayId, KeyActionConfig[] displayModels);
    void ClearMotionGroup(Guid motionId);
    KeyActionConfig[] GetDisplay(Guid motionId, Guid displayId);
}

using System;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IConflictKatMotionService
{
    bool IsConflict(Guid id, KatMotionEnum katMotion, KatPressModeEnum katPressMode, int count);
    void Register(KatMotionInfo katMotionInfo);
    void RemoveByGuid(Guid guid);
}

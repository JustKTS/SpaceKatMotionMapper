using System;
using System.Collections.Generic;
using System.Linq;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services;

public class ConflictKatMotionService
{
    private List<KatMotionInfo> OverwriteKatMotions { get; } = [];

    public bool IsConflict(Guid id, KatMotionEnum katMotion, KatPressModeEnum katPressMode, int count)
    {
        return OverwriteKatMotions.Any(e =>
            e.Id == id && e.Motion.Motion == katMotion && e.Motion.KatPressMode == katPressMode &&
            e.Motion.RepeatCount == count);
    }
    public void Register(KatMotionInfo katMotionInfo)
    {
        OverwriteKatMotions.Add(katMotionInfo);
    }

    public void RemoveByGuid(Guid guid)
    {
        OverwriteKatMotions.Where(e => e.Id == guid).ToList().Iter(e => OverwriteKatMotions.Remove(e));
    }
}
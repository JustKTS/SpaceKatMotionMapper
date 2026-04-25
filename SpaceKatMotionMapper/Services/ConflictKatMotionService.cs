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
        var target = new KatMotion(katMotion, katPressMode, count);
        return OverwriteKatMotions.Any(e =>
            e.Id == id && e.Motion.MatchesMotion(target));
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
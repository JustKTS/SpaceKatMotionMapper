using System;
using System.Collections.Generic;
using System.Linq;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services;

public class ConflictKatActionService
{
    private List<KatActionInfo> OverwriteKatActions { get; } = [];

    public bool IsConflict(Guid id, KatMotionEnum katMotion, KatPressModeEnum katPressMode, int count)
    {
        return OverwriteKatActions.Any(e =>
            e.Id == id && e.Action.Motion == katMotion && e.Action.KatPressMode == katPressMode &&
            e.Action.RepeatCount == count);
    }
    public void Register(KatActionInfo katActionInfo)
    {
        OverwriteKatActions.Add(katActionInfo);
    }

    public void RemoveByGuid(Guid guid)
    {
        OverwriteKatActions.Where(e => e.Id == guid).ToList().Iter(e => OverwriteKatActions.Remove(e));
    }
}
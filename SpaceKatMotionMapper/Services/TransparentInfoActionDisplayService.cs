using System;
using System.Collections.Generic;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Services;

public sealed class TransparentInfoActionDisplayService
{
    private readonly Dictionary<Guid, Dictionary<Guid, KeyActionConfig[]>> _motionGroup = [];

    public void Register(Guid motionId, Guid displayId, KeyActionConfig[] displayModels)
    {
        if (_motionGroup.TryGetValue(motionId, out var value))
        {
            value[displayId] = displayModels;
        }
        else
        {
            _motionGroup[motionId] = [];
            _motionGroup[motionId][displayId] = displayModels;
        }
    }

    public void ClearMotionGroup(Guid motionId)
    {
        _motionGroup[motionId] = [];
    }

    public KeyActionConfig[] GetDisplay(Guid motionId, Guid displayId)
    {
        try
        {
            return _motionGroup[motionId][displayId];
        }
        catch (Exception e)
        {
            return [new KeyActionConfig(ActionType.None, string.Empty, PressModeEnum.None, 0)];
        }
    }
}
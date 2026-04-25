using System;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.ViewModels.PressModePolicies;

namespace SpaceKatMotionMapper.Functions;

public class DelegatingPressModePolicy(Func<IKeyActionPressModePolicy> provider) : IKeyActionPressModePolicy
{
    public PressModeEnum GetDefaultPressMode(ActionType actionType) => provider().GetDefaultPressMode(actionType);
    public PressModeEnum CoercePressMode(ActionType actionType, PressModeEnum pressMode) => provider().CoercePressMode(actionType, pressMode);
}
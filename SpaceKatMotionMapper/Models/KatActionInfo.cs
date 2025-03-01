using System;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatActionInfo(
    Guid Id,
    KatAction Action);
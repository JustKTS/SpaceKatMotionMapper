using System;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Models;

public record KatDataWithInfo(bool ConfigIsDefault, Guid ActivatedConfigId, int Mode, KatAction KatAction);
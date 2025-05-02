using System.Collections.Generic;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Models;

public record KeyActionsForPresetRecord(string Description, List<KeyActionConfig> Actions);
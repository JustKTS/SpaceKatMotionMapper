using System;
using System.Collections.Generic;
using LanguageExt;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Functions.Contract;

public readonly record struct KatMotionSemanticItem(
    KatMotionEnum Motion,
    KatConfigModeEnum ConfigMode,
    IReadOnlyList<KeyActionConfig> Actions);

public readonly record struct KatMotionConfigSemanticValidationContext(
    IReadOnlyList<KatMotionSemanticItem> Items);

public interface IKatMotionConfigSemanticRule
{
    Either<Exception, bool> Validate(in KatMotionConfigSemanticValidationContext context);
}



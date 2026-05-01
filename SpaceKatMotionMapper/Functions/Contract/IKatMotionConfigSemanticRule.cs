using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
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
    Result<bool, Exception> Validate(in KatMotionConfigSemanticValidationContext context);
}


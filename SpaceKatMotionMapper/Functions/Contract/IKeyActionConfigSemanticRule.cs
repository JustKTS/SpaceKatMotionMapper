using System.Collections.Generic;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Functions.Contract;

public readonly record struct KeyActionSemanticValidationContext(
    IReadOnlyList<KeyActionConfig> Actions,
    bool IsSingleActionMode);

public interface IKeyActionConfigSemanticRule
{
    bool Validate(in KeyActionSemanticValidationContext context);
}


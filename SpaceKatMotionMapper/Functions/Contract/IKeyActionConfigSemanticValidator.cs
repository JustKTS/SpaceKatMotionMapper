using System.Collections.Generic;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Functions.Contract;

public interface IKeyActionConfigSemanticValidator
{
    bool Validate(IReadOnlyList<KeyActionConfig> actions, bool isSingleActionMode);
}

public class AlwaysPassKeyActionConfigSemanticValidator : IKeyActionConfigSemanticValidator
{
    public bool Validate(IReadOnlyList<KeyActionConfig> actions, bool isSingleActionMode)
    {
        return true;
    }
}



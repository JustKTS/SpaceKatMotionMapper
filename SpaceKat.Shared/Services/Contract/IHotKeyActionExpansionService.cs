using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Services.Contract;

public interface IHotKeyActionExpansionService
{
    IReadOnlyList<KeyActionConfig> Expand(CombinationKeysRecord combinationKeys, bool isSingleActionMode);
}


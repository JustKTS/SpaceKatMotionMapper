using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Services.Contract;

public interface IKeyActionExecutor
{
    // TODO: 修改以支持Linux
    void MouseActionHandler(MouseActionConfig mouseActionConfig);
    void KeyBoardActionHandler(KeyBoardActionConfig keyBoardActionConfig);
    void ExecuteActions(IEnumerable<KeyActionConfig> configs);
}
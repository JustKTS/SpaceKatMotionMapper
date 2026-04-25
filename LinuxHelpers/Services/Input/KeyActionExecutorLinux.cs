using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace LinuxHelpers.Services.Input;

public class KeyActionExecutorLinux : IKeyActionExecutor
{
    public void MouseActionHandler(MouseActionConfig mouseActionConfig)
    {
        // TODO: 处理失败的情况
        var ret = KeyExecutorYDoTool.ExecuteMouseAction(mouseActionConfig);
    }

    public void KeyBoardActionHandler(KeyBoardActionConfig keyBoardActionConfig)
    {
        KeyExecutorYDoTool.KeyBoardActionHandler(keyBoardActionConfig);
    }

    public void ExecuteActions(IEnumerable<KeyActionConfig> configs)
    {
        foreach (var actionConfig in configs)
        {
            if (actionConfig.TryToMouseActionConfig(out var mouseActionConfig))
            {
                MouseActionHandler(mouseActionConfig);
            }

            if (actionConfig.TryToKeyBoardActionConfig(out var keyboardActionConfig))
            {
                KeyBoardActionHandler(keyboardActionConfig);
            }

            if (actionConfig.TryToDelayActionConfig(out var delayActionConfig))
            {
                Thread.Sleep(delayActionConfig.Milliseconds);
            }
        }
    }
}

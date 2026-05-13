using Serilog;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

namespace LinuxHelpers.Services.Input;

public class KeyActionExecutorLinux : IKeyActionExecutor
{
    private static readonly ILogger Log = Serilog.Log.ForContext<KeyActionExecutorLinux>();

    public void MouseActionHandler(MouseActionConfig mouseActionConfig)
    {
        if (!KeyExecutorYDoTool.ExecuteMouseAction(mouseActionConfig))
            Log.Warning("鼠标模拟失败: {Action}", mouseActionConfig.Key);
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

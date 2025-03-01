using WindowsInput;

namespace SpaceKatMotionMapper.Models;

public record KeyBoardActionConfig(VirtualKeyCode Key, PressModeEnum PressMode)
{
    public KeyBoardActionConfig() : this(VirtualKeyCode.None, PressModeEnum.None)
    {
    }
}

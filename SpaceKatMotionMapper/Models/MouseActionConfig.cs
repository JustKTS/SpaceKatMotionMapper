namespace SpaceKatMotionMapper.Models;

public record MouseActionConfig(MouseButtonEnum Key, PressModeEnum PressMode, int Multiplier)
{
    public MouseActionConfig() : this(MouseButtonEnum.None, PressModeEnum.None, 1)
    {
    }
}

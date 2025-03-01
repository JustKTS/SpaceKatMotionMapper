namespace SpaceKatHIDWrapper.Models;

public record KatDeadZoneConfig(double[] Upper, double[] Lower)
{
    public KatDeadZoneConfig() : this([0.4,0.4,0.4,0.3,0.3,0.3], [-0.4,-0.4,-0.4,-0.3,-0.3,-0.3])
    {
    }
}
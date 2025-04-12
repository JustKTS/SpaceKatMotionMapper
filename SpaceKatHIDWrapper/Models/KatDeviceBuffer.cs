namespace SpaceKatHIDWrapper.Models;

public class KatDeviceBuffer
{
    private readonly double[] _axesData = [0,0,0,0,0,0];
    private readonly bool[] _buttons=[false, false];

    public void UpdateByAxis(MotionAxis axis, double value)
    {
        _axesData[(int)axis] = value;
    }

    public void UpdateButtonByIndex(int index, bool value)
    {
        _buttons[index] = value;
    }
    
    public KatDeviceData ToKatDeviceData()
    {
        return new KatDeviceData(DateTimeOffset.Now, _axesData, _buttons);
    }
}
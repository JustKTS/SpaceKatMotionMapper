namespace SpaceKatHIDWrapper.Models;

public class KatDeviceBuffer(double[] axesData, bool[] buttons)
{

    public KatDeviceBuffer(int buttonNums = 2) : this([0,0,0,0,0,0], new bool[buttonNums] )
    {
    }

    public void UpdateByAxis(MotionAxis axis, double value)
    {
        axesData[(int)axis] = value;
    }

    public void UpdateButtonByIndex(int index, bool value)
    {
        try
        {
            buttons[index] = value;
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"Index {index} is out of range");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public KatDeviceData ToKatDeviceData()
    {
        return new KatDeviceData(DateTimeOffset.Now, axesData, buttons);
    }
}
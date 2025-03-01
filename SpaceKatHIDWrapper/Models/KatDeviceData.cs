namespace SpaceKatHIDWrapper.Models;

public class KatDeviceData
{
    public DateTimeOffset Timestamp { get; set; }

    public double[] AxesData { get; } = new double[6];

    public double[] Translation
    {
        get => AxesData[..3];
        private set
        {
            if (value.Length != 3) return;
            AxesData[0] = value[0];
            AxesData[1] = value[1];
            AxesData[2] = value[2];
        }
    }

    public double[] Rotation
    {
        get => AxesData[3..];
        private set
        {
            if (value.Length != 3) return;
            AxesData[3] = value[0];
            AxesData[4] = value[1];
            AxesData[5] = value[2];
        }
    }

    public bool[] Buttons { get; set; }

    public KatDeviceData(DateTimeOffset timestamp, double[] axes,
        bool[] buttons)
    {
        Timestamp = timestamp;
        Translation = axes[..3];
        Rotation = axes[3..];
        Buttons = buttons;
    }

    public KatDeviceData() : this(DateTimeOffset.Now, new double[6], [])
    {
    }

    public bool UpdateByAxis(MotionAxis axis, double value)
    {
        try
        {
            AxesData[(int)axis] = value;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }


    public override string ToString()
    {
        var axesOut = AxesData.Select(x => $"{x:0.0000}").ToArray();
        return
            $"DeviceData {{ TimeStamp = {Timestamp.ToString()}, Tx = {axesOut[0]}, Ty = {axesOut[1]}, Tz ={axesOut[2]}, Roll = {axesOut[3]}, Pitch = {axesOut[4]}, Yaw = {axesOut[5]}, Buttons = {string.Join(",", Buttons)} }}\n";
    }
}
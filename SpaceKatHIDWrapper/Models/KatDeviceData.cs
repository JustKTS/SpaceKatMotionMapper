namespace SpaceKatHIDWrapper.Models;

public record KatDeviceData(DateTimeOffset Timestamp, double[] AxesData, bool[] Buttons)
{
    public double[] Translation => AxesData[..3];

    public double[] Rotation => AxesData[3..];

    public override string ToString()
    {
        var axesOut = AxesData.Select(x => $"{x:0.0000}").ToArray();
        return
            $"DeviceData {{ TimeStamp = {Timestamp.ToString()}, Tx = {axesOut[0]}, Ty = {axesOut[1]}, Tz ={axesOut[2]}, Roll = {axesOut[3]}, Pitch = {axesOut[4]}, Yaw = {axesOut[5]}, Buttons = {string.Join(",", Buttons)} }}\n";
    }
}
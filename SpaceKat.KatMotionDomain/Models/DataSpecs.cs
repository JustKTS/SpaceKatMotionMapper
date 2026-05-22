namespace SpaceKatHIDWrapper.Models;

public record AxisSpecs(byte Channel, int ByteIndex1, int ByteIndex2, double Flip);

public record ButtonSpecs(byte Channel, int ByteIndex, byte Bit);

public enum MotionAxis
{
    X = 0,
    Y = 1,
    Z = 2,
    Roll = 3,
    Pitch = 4,
    Yaw = 5
}


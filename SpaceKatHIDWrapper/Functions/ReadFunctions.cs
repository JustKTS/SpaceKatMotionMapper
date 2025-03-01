using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.Functions;

public static class ReadFunctions
{
    public static bool UpdateDeviceData(ReadOnlySpan<byte> rawData,  ref KatDeviceData katDeviceData,
        FrozenDictionary<MotionAxis, AxisSpecs> axesMappings, double axesScale, ReadOnlyCollection<ButtonSpecs> buttonsMappings)
    {
        try
        {
            foreach (var (axisName, axisSpecs) in axesMappings)
            {
                if (rawData[0] != axisSpecs.Channel) continue;
                if (axisSpecs.ByteIndex1 > rawData.Length || axisSpecs.ByteIndex2 > rawData.Length) continue;

                katDeviceData.UpdateByAxis(axisName,
                    BitConverter.ToInt16(rawData[axisSpecs.ByteIndex1 .. (axisSpecs.ByteIndex2 + 1)]) * axisSpecs.Flip /
                    axesScale);
            }
            
            List<bool> buttons = [];
            foreach (var buttonMapping in buttonsMappings)
            {
                if (rawData[0] != buttonMapping.Channel) continue;
                var mask = 1U << buttonMapping.Bit;
                buttons.Add((rawData[buttonMapping.ByteIndex] & mask) != 0);
            }
            katDeviceData.Buttons = buttons.ToArray();
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static int GetReadBytesCount(FrozenDictionary<MotionAxis, AxisSpecs> axisMappings)
    {
        return axisMappings.Values.SelectMany(axis => new[] { axis.ByteIndex1, axis.ByteIndex2 }).Max() + 1;
    }
}
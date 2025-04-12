using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.Functions;

public static class ReadFunctions
{
    public static bool UpdateDeviceData(ReadOnlySpan<byte> rawData,
        in KatDeviceBuffer buffer,
        FrozenDictionary<MotionAxis, AxisSpecs> axesMappings, double axesScale,
        ReadOnlyCollection<ButtonSpecs> buttonsMappings)
    {
        try
        {
            foreach (var (axisName, axisSpecs) in axesMappings)
            {
                if (rawData[0] != axisSpecs.Channel) continue;
                if (axisSpecs.ByteIndex1 > rawData.Length || axisSpecs.ByteIndex2 > rawData.Length) continue;

                buffer.UpdateByAxis(axisName,
                    BitConverter.ToInt16(rawData[axisSpecs.ByteIndex1 .. (axisSpecs.ByteIndex2 + 1)]) * axisSpecs.Flip /
                    axesScale);
            }

            for (var i = 0; i < buttonsMappings.Count; i++)
            {
                if (rawData[0] != buttonsMappings[i].Channel) continue;
                var mask = 1U << buttonsMappings[i].Bit;
                buffer.UpdateButtonByIndex(i, (rawData[buttonsMappings[i].ByteIndex] & mask) != 0);
            }


            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public static int GetReadBytesCount(FrozenDictionary<MotionAxis, AxisSpecs> axisMappings)
    {
        return axisMappings.Values.SelectMany(axis => new[] { axis.ByteIndex1, axis.ByteIndex2 }).Max() + 1;
    }
}
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using HidApi;
using SpaceKatHIDWrapper.Functions;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceWrappers;

public class SpaceCompatDataWrapper : IDeviceDataWrapper
{
    private const string Name = "SpaceMouse Compact";

    public event EventHandler<bool>? ConnectionChanged;
    
    // vendor ID and product ID
    private int[] _hidId = [0x256F, 0xC635];

    // private readonly FrozenDictionary<MotionAxis, AxisSpecs> _axesMappings = new Dictionary<MotionAxis, AxisSpecs>()
    // {
    //     [MotionAxis.X] = new(1, 1, 2, 1),
    //     [MotionAxis.Y] = new(1, 3, 4, -1),
    //     [MotionAxis.Z] = new(1, 5, 6, -1),
    //     [MotionAxis.Pitch] = new(2, 1, 2, -1),
    //     [MotionAxis.Roll] = new(2, 3, 4, -1),
    //     [MotionAxis.Yaw] = new(2, 5, 6, 1),
    // }.ToFrozenDictionary();
    
    private readonly FrozenDictionary<MotionAxis, AxisSpecs> _axesMappings = new Dictionary<MotionAxis, AxisSpecs>()
    {
        [MotionAxis.X] = new(1, 1, 2, 1),
        [MotionAxis.Y] = new(1, 3, 4, -1),
        [MotionAxis.Z] = new(1, 5, 6, -1),
        [MotionAxis.Pitch] = new(1, 7, 8, -1),
        [MotionAxis.Roll] = new(1, 9, 10, -1),
        [MotionAxis.Yaw] = new(1, 11, 12, 1),
    }.ToFrozenDictionary();


    private readonly ReadOnlyCollection<ButtonSpecs> _buttonMappings = new List<ButtonSpecs>
    {
        new(3, 1, 0), // LEFT
        new(3, 1, 1) // RIGHT
    }.AsReadOnly();

    private const double AxisScale = 350.0;

    private Device? _device;

    private KatDeviceData _katDeviceData = new();

    public bool IsConnected => _device != null;

    public void SetDevice(Device device)
    {
        _device = device;
        ConnectionChanged?.Invoke(this, IsConnected);
    }

    private readonly int _readLength;

    public SpaceCompatDataWrapper()
    {
        _readLength = ReadFunctions.GetReadBytesCount(_axesMappings);
    }

    public KatDeviceData? Read()    
    {
        if (_device == null) return null;

        try
        {
            var rawData =_device.ReadTimeout(_readLength, 200);
            // var rawData = _device.Read(_readLength);

            ReadFunctions.UpdateDeviceData(rawData, ref _katDeviceData, _axesMappings, AxisScale, _buttonMappings);
            
            return _katDeviceData;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Hid.Exit();
            _device = null;
            ConnectionChanged?.Invoke(this, IsConnected);
            return null;
        }
    }
}
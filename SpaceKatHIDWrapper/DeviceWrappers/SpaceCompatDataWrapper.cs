using System.Collections.Frozen;
using System.Collections.ObjectModel;
using HidApi;
using LanguageExt;
using SpaceKatHIDWrapper.Functions;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceWrappers;

public class SpaceCompatDataWrapper : IDeviceDataWrapper
{
    private const string Name = "SpaceMouse Compact";
    public event EventHandler<Either<Exception, bool>>? ConnectionChanged;

    // vendor ID and product ID
    private readonly int[] _hidId = [0x256F, 0xC635];

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
    
    private int _readFailedCount;

    public bool IsConnected => _device != null;
    
    private readonly KatDeviceBuffer _buffer = new();

    private void RetryConnect(object? obj, Either<Exception, bool> isConnected)
    {
        if (isConnected.IsRight) return;
        const int maxRetry = 5;
        Task.Run(async () =>
        {
            for (var i = 0; i < maxRetry; i++)
            {
                await Task.Delay(2000);
                var device = await KatDeviceFunction.FindKatDeviceById(_hidId[0], _hidId[1]);
                if (device == null) continue;
                return device;
            }

            return null;
        }).ContinueWith(t =>
        {
            if (t.Result == null) return;
            _device = t.Result;
            ConnectionChanged?.Invoke(this, IsConnected);
        });
    }

    public async Task<bool> Connect()
    {
        var device = await KatDeviceFunction.FindKatDeviceById(_hidId[0], _hidId[1]);
        if (device == null) return false;
        _device = device;
        ConnectionChanged?.Invoke(this, IsConnected);
        return true;
    }

    private readonly int _readLength;

    public SpaceCompatDataWrapper()
    {
        _readLength = ReadFunctions.GetReadBytesCount(_axesMappings);
        ConnectionChanged += RetryConnect;
    }

    public KatDeviceData? Read()
    {
        if (_device == null) return null;

        try
        {
            var rawData = _device.ReadTimeout(_readLength, 300);
            
            var ret = ReadFunctions.UpdateDeviceData(rawData, in _buffer, _axesMappings, AxisScale, _buttonMappings);
            return ret? _buffer.ToKatDeviceData() : null;
        }
        catch (Exception e)
        {
            _readFailedCount += 1;
            if (_readFailedCount < 5) return null;
            Console.WriteLine(e);
            Hid.Exit();
            _device = null;
            ConnectionChanged?.Invoke(this, e);
            _readFailedCount = 0;
            return null;
        }
    }

    public void Disconnect()
    {
        KatDeviceFunction.StopDevice();
        _device?.Dispose();
        _device = null;
        ConnectionChanged?.Invoke(this, IsConnected);
    }
}
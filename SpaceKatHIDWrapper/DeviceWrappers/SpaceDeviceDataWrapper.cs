using HidApi;
using LanguageExt;
using SpaceKatHIDWrapper.DeviceHIDSpecs;
using SpaceKatHIDWrapper.Functions;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceWrappers;

public class SpaceDeviceDataWrapper : IDeviceDataWrapper
{
    private DeviceHidSpec? _hidSpec;
    public event EventHandler<Either<Exception, bool>>? ConnectionChanged;


    private Device? _device;

    private int _readFailedCount;

    public bool IsConnected => _device != null;

    private KatDeviceBuffer? _buffer;

    private void RetryConnect(object? obj, Either<Exception, bool> isConnected)
    {
        if (isConnected.IsRight) return;
        const int maxRetry = 5;
        Task.Run(async () =>
            {
                for (var i = 0; i < maxRetry; i++)
                {
                    await Task.Delay(2000);
                    var ret = await Connect();
                    if (ret)
                    {
                        ConnectionChanged?.Invoke(this, IsConnected);
                        return;
                    }
                }
            }
        );
    }

    public async Task<bool> Connect()
    {
        var devicePack = await KatDeviceFunction.FindKatDevice();
        return devicePack.Match((pair) =>
        {
            var (spec, device) = pair;
            _hidSpec = spec;
            _device = device;
            _buffer = new KatDeviceBuffer(_hidSpec.ButtonMappings.Count);
            _readLength = ReadFunctions.GetReadBytesCount(_hidSpec.AxesMappings);
            ConnectionChanged?.Invoke(this, IsConnected);
            return true;
        }, _ => false);
    }

    private int _readLength;

    public SpaceDeviceDataWrapper()
    {
        ConnectionChanged += RetryConnect;
    }

    public KatDeviceData? Read()
    {
        if (_device == null) return null;
        if (_hidSpec == null) return null;

        try
        {
            var rawData = _device.ReadTimeout(_readLength, 300);
            if  (_buffer == null) return null;
            var ret = ReadFunctions.UpdateDeviceData(rawData, in _buffer, _hidSpec.AxesMappings, _hidSpec.AxisScale,
                _hidSpec.ButtonMappings);
            _readFailedCount = 0;
            return ret ? _buffer.ToKatDeviceData() : null;
        }
        catch (Exception e)
        {
            _readFailedCount += 1;
            if (_readFailedCount < 5) return null;
            Console.WriteLine(e);
            _device?.Dispose();
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
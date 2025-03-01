using HidApi;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceWrappers;

public interface IDeviceDataWrapper
{
    public bool IsConnected { get; }
    public void SetDevice(Device device);
    public KatDeviceData? Read();

    public event EventHandler<bool>? ConnectionChanged;

}
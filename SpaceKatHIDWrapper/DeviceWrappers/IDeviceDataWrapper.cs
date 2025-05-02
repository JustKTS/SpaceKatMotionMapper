using HidApi;
using LanguageExt;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceWrappers;

public interface IDeviceDataWrapper
{
    public bool IsConnected { get; }
    public Task<bool> Connect();
    public void Disconnect();
    public KatDeviceData? Read();

    public event EventHandler<Either<Exception, bool>>? ConnectionChanged;

}
using HidApi;

namespace SpaceKatHIDWrapper.Functions;

public static class KatDeviceFunction
{
    public static async Task<Device?> FindKatDevice()
    {
        var devices = await Task.Run(() => Hid.Enumerate());
        return devices.Select(deviceInfo => deviceInfo.ConnectToDevice())
            .FirstOrDefault(device => device.GetProduct().Contains("SPK"));
    }

    public static void StopDevice()
    {
        Hid.Exit();
    }
}
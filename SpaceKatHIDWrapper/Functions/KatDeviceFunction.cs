using HidApi;

namespace SpaceKatHIDWrapper.Functions;

public static class KatDeviceFunction
{
    public static async Task<Device?> FindKatDevice()
    {
        var devices = await Task.Run(() => Hid.Enumerate());

        // ReSharper disable once StringLiteralTypo
        return (devices.Where(deviceInfo => deviceInfo.ManufacturerString == "FIRESTAR")
            .Select(deviceInfo => deviceInfo.ConnectToDevice())).FirstOrDefault();
    }

    public static void StopDevice()
    {
        Hid.Exit();
    }
}
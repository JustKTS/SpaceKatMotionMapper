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

    public static async Task<Device?> FindKatDeviceById(int vendorId, int productId)
    {
        var devices = await Task.Run(() => Hid.Enumerate());
        try
        {
            return devices.Where(deviceInfo => deviceInfo.VendorId == vendorId && deviceInfo.ProductId == productId)
                .Select(deviceInfo => deviceInfo.ConnectToDevice()).FirstOrDefault();
        }
        catch (Exception e)
        {
            return null;
        }
       
    }

    public static void StopDevice()
    {
        Hid.Exit();
    }
}
using HidApi;
using LanguageExt;
using SpaceKatHIDWrapper.DeviceHIDSpecs;

namespace SpaceKatHIDWrapper.Functions;

public static class KatDeviceFunction
{
    public static async Task<Device?> FindKatDeviceById(int vendorId, int productId)
    {
        var devices = await Task.Run(() => Hid.Enumerate());
        try
        {
            return devices.Where(deviceInfo => deviceInfo.VendorId == vendorId && deviceInfo.ProductId == productId)
                .Select(deviceInfo => deviceInfo.ConnectToDevice()).FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<Either<Exception, (DeviceHidSpec, Device)>> FindKatDevice()
    {
        foreach (var spec in DeviceHidSpecDict.HidIds.Values)
        {
            var device = await FindKatDeviceById(spec.HidId[0], spec.HidId[1]);
            if (device != null)
            {
                return (spec, device);
            }
        }
        return new Exception("未找到设备");
    }

    public static void StopDevice()
    {
    }
}
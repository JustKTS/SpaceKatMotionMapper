namespace PlatformAbstractions.Unsupported;

public class UnsupportedPlatformHotKeyService : IPlatformHotKeyService
{
    public bool IsSupported => false;

    public bool RegisterHotKeyWrapper(nint handle, int id, int fsModifiers, int vk)
    {
        return false;
    }

    public bool UnregisterHotKeyWrapper(nint handle, int id)
    {
        return false;
    }
}

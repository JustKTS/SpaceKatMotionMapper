namespace PlatformAbstractions.Unsupported;

public class UnsupportedAutostartService : IPlatformAutostartService
{
    public bool IsAutostartEnabled
    {
        get => false;
        set { }
    }

    public bool IsAvailable => false;
}

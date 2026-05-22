namespace PlatformAbstractions;

public interface IPlatformAutostartService
{
    bool IsAutostartEnabled { get; set; }

    bool IsAvailable { get; }
}

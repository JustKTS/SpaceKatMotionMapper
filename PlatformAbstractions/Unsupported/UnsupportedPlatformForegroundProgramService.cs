using System;

namespace PlatformAbstractions.Unsupported;

public class UnsupportedPlatformForegroundProgramService : IPlatformForegroundProgramService
{
    public bool IsSupported => false;

    public event EventHandler<ForeProgramInfo>? ForeProgramChanged
    {
        add { }
        remove { }
    }

    public void Dispose()
    {
    }
}

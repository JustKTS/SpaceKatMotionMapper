using System;

namespace SpaceKat.Shared.Services.Contract;

public interface IGlobalStates
{
    bool IsConnected { get; set; }
    bool IsMapperEnable { get; set; }
    bool IsTransparentInfoEnable { get; set; }
    event EventHandler<bool>? IsConnectionChanged;
    event EventHandler<bool>? IsMapperEnableChanged;
    event EventHandler<bool>? IsTransparentInfoEnableChanged;
}

using System;
using PlatformAbstractions;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IModeChangeService
{
    int CurrentMode { get; set; }
    bool ConfigIsDefault { get; }
    Guid CurrentActivatedConfig { get; }
    bool IsPlatformSupported { get; }
    ForeProgramInfo? CurrentForeProgramInfo { get; }
    void UpdateBindProcessPathList(KatMotionConfigGroup configGroup);
    void RemovePathForBindProcessPathList(string processPath);
}

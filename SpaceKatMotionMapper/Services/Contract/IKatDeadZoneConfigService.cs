using System;
using CSharpFunctionalExtensions;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKatDeadZoneConfigService
{
    KatDeadZoneConfig LoadDefaultDeadZoneConfigs();
    KatDeadZoneConfig? LoadDeadZoneConfigs(Guid configGroupId);
    Result<bool, Exception> SaveDefaultDeadZoneConfig(KatDeadZoneConfig deadZoneConfig);
    Result<bool, Exception> SaveDeadZoneConfig(KatDeadZoneConfig deadZoneConfig, Guid configGroupId);
    bool ApplyDeadZoneConfigById(Guid id);
    bool ApplyDefaultDeadZoneConfig();
}

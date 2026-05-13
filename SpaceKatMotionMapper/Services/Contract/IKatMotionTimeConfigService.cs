using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKatMotionTimeConfigService
{
    KatMotionTimeConfigs LoadDefaultTimeConfigs();
    KatMotionTimeConfigs? LoadMotionTimeConfigs(Guid configGroupId);
    Result<bool, Exception> SaveDefaultTimeConfig(KatMotionTimeConfigs timeConfig);
    Result<bool, Exception> SaveTimeConfig(KatMotionTimeConfigs timeConfig, Guid configGroupId);
    bool ApplyMotionTimeConfigById(Guid id);
    bool ApplyDefaultMotionTimeConfig();
    HashSet<KatMotionEnum> GetSingleActionMotionsFromDefaultConfig();
    HashSet<KatMotionEnum> GetSingleActionMotionsById(Guid configGroupId);
}

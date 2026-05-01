using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKatMotionFileService
{
    Result<bool, Exception> SaveDefaultConfigGroup(KatMotionConfigGroup configGroup);
    Result<KatMotionConfigGroup, Exception> LoadDefaultConfigGroup();
    Result<KatMotionConfigGroup, Exception> LoadConfigGroup(string configFilePath);
    Result<List<KatMotionConfigGroup>, Exception> LoadConfigGroupsFromSysConf();
    Result<bool, Exception> SaveConfigGroupToSysConf(KatMotionConfigGroup configGroup);
    Result<bool, Exception> SaveConfigGroup(KatMotionConfigGroup configGroup, string configFilePath);
    Result<bool, Exception> SaveConfigGroupsToSysConf(IEnumerable<KatMotionConfigGroup> configGroups);
    Result<bool, Exception> DeleteConfigGroupFromSysConf(Guid id);
}

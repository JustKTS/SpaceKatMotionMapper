using System;
using System.Collections.Generic;
using LanguageExt;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKatMotionFileService
{
    Either<Exception, bool> SaveDefaultConfigGroup(KatMotionConfigGroup configGroup);
    Either<Exception, KatMotionConfigGroup> LoadDefaultConfigGroup();
    Either<Exception, KatMotionConfigGroup> LoadConfigGroup(string configFilePath);
    Either<Exception, List<KatMotionConfigGroup>> LoadConfigGroupsFromSysConf();
    Either<Exception, bool> SaveConfigGroupToSysConf(KatMotionConfigGroup configGroup);
    Either<Exception, bool> SaveConfigGroup(KatMotionConfigGroup configGroup, string configFilePath);
    Either<Exception, bool> SaveConfigGroupsToSysConf(IEnumerable<KatMotionConfigGroup> configGroups);
    Either<Exception, bool> DeleteConfigGroupFromSysConf(Guid id);
}

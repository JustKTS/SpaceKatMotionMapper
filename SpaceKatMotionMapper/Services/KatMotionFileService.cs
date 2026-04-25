using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using SpaceKatMotionMapper.Defines;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using Exception = System.Exception;

namespace SpaceKatMotionMapper.Services;

public class KatMotionFileService : IKatMotionFileService
{
    private readonly string _configDirPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SpaceKatMotionMapper");

    private readonly string _customConfigDirPath;

    private const string CustomConfigDirName = "CustomConfigs";
    private const string DefaultConfigFileName = "DefaultConfigGroup.json";
    private readonly IFileService _fileService;

    public KatMotionFileService(IFileService fileService)
    {
        _fileService = fileService;
        _customConfigDirPath = Path.Combine(_configDirPath, CustomConfigDirName);
        if (!Directory.Exists(_configDirPath))
        {
            Directory.CreateDirectory(_configDirPath);
        }

        if (!Directory.Exists(_customConfigDirPath))
        {
            Directory.CreateDirectory(_customConfigDirPath);
        }
    }

    public Either<Exception, bool> SaveDefaultConfigGroup(KatMotionConfigGroup configGroup)
    {
        try
        {
            _fileService.Save(_configDirPath, DefaultConfigFileName, configGroup);
            return true;
        }
        catch (Exception)
        {
            return new Exception("保存配置文件失败");
        }
    }

    public Either<Exception, KatMotionConfigGroup> LoadDefaultConfigGroup()
    {
        try
        {
            return LoadConfigGroup(Path.Combine(_configDirPath, DefaultConfigFileName));
        }
        catch (Exception e)
        {
            return new Exception("读取配置文件失败", innerException: e);
        }
    }

    public Either<Exception, KatMotionConfigGroup> LoadConfigGroup(string configFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(configFilePath);
            if (dir is null) return new Exception("输入地址有误，无法提取文件夹");
            var fileName = Path.GetFileName(configFilePath);
            var configGroupMayNull = _fileService.Read<KatMotionConfigGroup>(dir, fileName);
            if (configGroupMayNull is not { } configGroup)
                return new Exception("读取配置文件失败");

            while (configGroup.Version != GlobalConstConfigs.ConfigFileVersion)
            {
                configGroup = configGroup.Version switch
                {
                    0 => TreatV1ToV2ConfigGroup(configGroup),
                    1 => TreatV1ToV2ConfigGroup(configGroup),
                    2 => TreatV2ToV3ConfigGroup(configGroup),
                    3 => TreatV3ToV4ConfigGroup(configGroup),
                    _ => throw new InvalidDataException($"不支持的配置版本: {configGroup.Version}")
                };
            }

            return configGroup;
        }
        catch (Exception e)
        {
            return new Exception("读取配置文件失败", innerException: e);
        }
    }

    public Either<Exception, List<KatMotionConfigGroup>> LoadConfigGroupsFromSysConf()
    {
        try
        {
            var configFiles = Directory.GetFiles(_customConfigDirPath, "*.json");

            List<KatMotionConfigGroup> configGroups = [];
            foreach (var configFile in configFiles)
            {
                // TODO: 处理部分文件读取失败的情况
                LoadConfigGroup(Path.Combine(_customConfigDirPath, configFile))
                    .Iter(configGroups.Add);
            }

            return configGroups;
        }
        catch (Exception e)
        {
            return new Exception("读取配置文件错误", innerException: e);
        }
    }

    public Either<Exception, bool> SaveConfigGroupToSysConf(KatMotionConfigGroup configGroup)
    {
        var fileName = configGroup.Guid + ".json";
        var path = Path.Combine(_customConfigDirPath, fileName);
        return SaveConfigGroup(configGroup, path);
    }

    public Either<Exception, bool> SaveConfigGroup(KatMotionConfigGroup configGroup, string configFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(configFilePath);
            if (string.IsNullOrEmpty(dir))
            {
                return new ArgumentException($"输入的地址有误，请检查！错误地址：{configFilePath}");
            }

            var fileName = Path.GetFileName(configFilePath);
            _fileService.Save(dir, fileName, configGroup);
            return true;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public Either<Exception, bool> SaveConfigGroupsToSysConf(IEnumerable<KatMotionConfigGroup> configGroups)
    {
        foreach (var configGroup in configGroups)
        {
            var ret = SaveConfigGroupToSysConf(configGroup)
                .Match<Either<Exception, bool>>(flag => flag, exception => exception);
            if (ret.IsLeft)
            {
                return ret;
            }
        }

        return true;
    }

    public Either<Exception, bool> DeleteConfigGroupFromSysConf(Guid id)
    {
        try
        {
            var configFilePath = Path.Combine(_customConfigDirPath, id + ".json");
            if (!File.Exists(configFilePath)) return new Exception("找不到配置文件");
            File.Delete(configFilePath);
            return true;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    #region 旧版本兼容处理

    private static KatMotionConfigGroup TreatV1ToV2ConfigGroup(KatMotionConfigGroup configGroup)
    {
        return configGroup with { Version = 2 };
        // return configGroup with { Version = 2, Motions = motionConfigs.ToList() };
    }

    private static KatMotionConfigGroup TreatV2ToV3ConfigGroup(KatMotionConfigGroup configGroup)
    {
        // 将所有V2配置迁移为进阶模式
        var migratedMotions = configGroup.Motions.Select(
            motion =>
                new KatMotionConfig(
                    motion.Motion,
                    motion.ActionConfigs,
                    motion.IsCustomDescription,
                    motion.KeyActionsDescription,
                    motion.ModeNum,
                    motion.ToModeNum))
                .ToList();

        return configGroup with
        {
            Version = 3,
            Motions = migratedMotions
        };
    }

    private static KatMotionConfigGroup TreatV3ToV4ConfigGroup(KatMotionConfigGroup configGroup)
    {
        // V3→V4: 添加默认长推触发时间、默认单动作长推触发时间、默认重复触发放大系数
        configGroup.MotionTimeConfigs.DefaultLongReachTimeoutMs = 300;
        configGroup.MotionTimeConfigs.DefaultSingleActionLongReachTimeoutMs = 50;
        configGroup.MotionTimeConfigs.DefaultRepeatScaleFactor = 1.5;

        return configGroup with { Version = 4 };
    }

    #endregion
}
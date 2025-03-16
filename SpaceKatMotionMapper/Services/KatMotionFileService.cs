using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LanguageExt.Common;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class KatMotionFileService
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

    public Result<bool> SaveDefaultConfigGroup(KatMotionConfigGroup configGroup)
    {
        try
        {
            _fileService.Save(_configDirPath, DefaultConfigFileName, configGroup);
            return true;
        }
        catch (Exception e)
        {   
            Debug.WriteLine(e);
            return new Result<bool>(new Exception("保存配置文件失败"));
        }
    }

    public Result<KatMotionConfigGroup> LoadDefaultConfigGroup()
    {
        try
        {
            var ret = _fileService.Read<KatMotionConfigGroup>(_configDirPath, DefaultConfigFileName);
            return ret ?? new Result<KatMotionConfigGroup>(new Exception("读取配置文件失败"));
        }
        catch (Exception e)
        {
            return new Result<KatMotionConfigGroup>(new Exception("读取配置文件失败", innerException: e));
        }
    }

    public Result<KatMotionConfigGroup> LoadConfigGroup(string configFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(configFilePath);
            if (dir is null) return new Result<KatMotionConfigGroup>(new Exception("输入地址有误，无法提取文件夹"));
            var fileName = Path.GetFileName(configFilePath);
            var configGroup = _fileService.Read<KatMotionConfigGroup>(dir, fileName);
            return configGroup ?? new Result<KatMotionConfigGroup>(new Exception("读取配置文件失败"));
        }
        catch (Exception e)
        {
            return new Result<KatMotionConfigGroup>(new Exception("读取配置文件失败", innerException:e));
        }
    }
    
    public Result<List<KatMotionConfigGroup>> LoadConfigGroupsFromSysConf()
    {
        try
        {
            var configFiles = Directory.GetFiles(_customConfigDirPath, "*.json");
            
            List<KatMotionConfigGroup> configGroups = [];
            foreach (var configFile in configFiles)
            {
                var ret = LoadConfigGroup(Path.Combine(_customConfigDirPath, configFile));
                _ = ret.Match((configGroup) =>
                {
                    configGroups.Add(configGroup);
                    return new Result<bool>(true);
                }, exception => new Result<bool>(false));
            }
            return configGroups;
        }
        catch (Exception e)
        {
            return new Result<List<KatMotionConfigGroup>>(new Exception("读取配置文件错误", innerException:e));
        }
    }

    public Result<bool> SaveConfigGroupToSysConf(KatMotionConfigGroup configGroup)
    {
        var fileName = configGroup.Guid + ".json";
        var path = Path.Combine(_customConfigDirPath, fileName);
        return SaveConfigGroup(configGroup, path);
    }
    
    public Result<bool> SaveConfigGroup(KatMotionConfigGroup configGroup, string configFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(configFilePath);
            if (string.IsNullOrEmpty(dir))
            {
                return new Result<bool>(new ArgumentException($"输入的地址有误，请检查！错误地址：{configFilePath}"));
            }
            var fileName = Path.GetFileName(configFilePath);
            _fileService.Save(dir, fileName, configGroup);
            return true;
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }
    
    public Result<bool> SaveConfigGroupsToSysConf(IEnumerable<KatMotionConfigGroup> configGroups)
    {
        var retFlag = true;
        foreach (var configGroup in configGroups)
        {
            var ret = SaveConfigGroupToSysConf(configGroup);
            _ = ret.Match<Result<bool>>(flag => flag, exception =>
            {
                retFlag = false;
                return false;
            });
        }
        return retFlag;
    }

    public Result<bool> DeleteConfigGroupFromSysConf(Guid id)
    {
        try
        {
            var configFilePath = Path.Combine(_customConfigDirPath, id + ".json");
            if (!File.Exists(configFilePath)) return new Result<bool>(new Exception("找不到配置文件"));
            File.Delete(configFilePath);
            return true;
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }
}
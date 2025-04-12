using System.Text;
using System.Text.Json;
using LanguageExt.Common;
using Serilog;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.States;

namespace SpaceKat.Shared.Services;

public class ProgramSpecMetaKeyFileService : IProgramSpecMetaKeyFileService
{
    public ProgramSpecMetaKeyFileService()
    {
        if (!Directory.Exists(GlobalPaths.AppDataPath))
        {
            Directory.CreateDirectory(GlobalPaths.AppDataPath);
        }

        if (!Directory.Exists(GlobalPaths.MetaKeysConfigPath))
        {
            Directory.CreateDirectory(GlobalPaths.MetaKeysConfigPath);
        }
    }

    public Result<bool> SaveToConfigDir(ProgramSpecMetaKeysRecord config)
    {
        try
        {
            var configFilePath = Path.Combine(GlobalPaths.MetaKeysConfigPath, $"{config.ConfigName}.json");
            return SaveToFile(config, configFilePath);
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }

    public Result<bool> SaveToFile(ProgramSpecMetaKeysRecord config, string filepath)
    {
        try
        {
            var fileContent = JsonSerializer.Serialize(config,
                ProgramSpecJsOption.Default);
            File.WriteAllText(filepath, fileContent, Encoding.UTF8);
            return true;
        }
        catch (Exception e)
        {
            return new Result<bool>(e);
        }
    }

    public Dictionary<string, ProgramSpecMetaKeysRecord> LoadConfigs()
    {
        var configFiles = Directory.GetFiles(GlobalPaths.MetaKeysConfigPath, "*.json");
        Dictionary<string, ProgramSpecMetaKeysRecord> configs = [];
        foreach (var configFilename in configFiles)
        {
            var jsonRaw = File.ReadAllText(configFilename);
            try
            {
                var config =
                    JsonSerializer.Deserialize<ProgramSpecMetaKeysRecord>(jsonRaw,
                        ProgramSpecJsOption.Default);
                if (config == null)
                {
                    Log.Logger.Warning("程序快捷键配置文件 {configFilename} 有误, 请重新保存或删除", configFilename);
                    continue;
                }

                configs.Add(config.ConfigName, config);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "");
            }
        }

        return configs;
    }
}
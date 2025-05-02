using System.Text;
using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using Serilog;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.States;

namespace SpaceKat.Shared.Services;

public class MetaKeyPresetFileService : IMetaKeyPresetFileService
{
    public MetaKeyPresetFileService()
    {
        if (!Directory.Exists(GlobalPaths.AppDataPath))
        {
            Directory.CreateDirectory(GlobalPaths.AppDataPath);
        }

        if (!Directory.Exists(GlobalPaths.MetaKeysConfigPath))
        {
            Directory.CreateDirectory(GlobalPaths.MetaKeysConfigPath);
        }

        if (File.Exists(Path.Combine(GlobalPaths.MetaKeysConfigPath, "我的收藏.json"))) return;
        var favConfig = new ProgramSpecMetaKeysRecord("我的收藏", string.Empty, true, [], []);
        SaveToConfigDir(favConfig);
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

    public Either<Exception, Dictionary<string, ProgramSpecMetaKeysRecord>> LoadConfigs()
    {
        var configFiles = Directory.GetFiles(GlobalPaths.MetaKeysConfigPath, "*.json");
        Dictionary<string, ProgramSpecMetaKeysRecord> configs = [];
        try
        {
            foreach (var configFilename in configFiles)
            {
                var configOption = LoadFromFile(configFilename);
                configOption.Iter(config => { configs.Add(config.ConfigName, config); });
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "");
        }

        return configs.Count == 0 ? new Exception("未找到任何配置文件") : configs;
    }


    public Either<Exception, ProgramSpecMetaKeysRecord> LoadFromFile(string filepath)
    {
        var filename = Path.GetFileName(filepath);
        var jsonRaw = File.ReadAllText(filepath);
        try
        {
            var config =
                JsonSerializer.Deserialize<ProgramSpecMetaKeysRecord>(jsonRaw,
                    ProgramSpecJsOption.Default);
            if (config != null) return config;
            return new Exception($"程序快捷键配置文件 {filename} 有误, 请重新保存或删除");
        }
        catch (Exception e)
        {
            return new Exception($"程序快捷键配置文件 {filename} 有误, {e.Message}");
        }
    }
}
using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Serilog;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class MetaKeyPresetService : IMetaKeyPresetService
{
    private readonly IMetaKeyPresetFileService _metaKeyPresetFileService;
    private readonly ILogger _logger;
    public Dictionary<string, ProgramSpecMetaKeysRecord> Configs { get; private set; } = [];

    public MetaKeyPresetService(IMetaKeyPresetFileService metaKeyPresetFileService, ILogger logger)
    {
        _metaKeyPresetFileService = metaKeyPresetFileService;
        _logger = logger;
        ReloadConfigs();
    }
    public void ReloadConfigs()
    {
        var result = _metaKeyPresetFileService.LoadConfigs();
        if (result.IsSuccess)
            Configs = result.Value;
        else
            _logger.Error(result.Error, "");
    }

    public Result<bool, Exception> AddToFavPreset(string description, List<KeyActionConfig> keyActionConfigs)
    {
        var config = Configs["我的收藏"];
        var macroKeys = config.MacroKeys;
        if (!macroKeys.TryAdd(description, keyActionConfigs))
        {
            return new Exception($"已存在名为 \"{description}\" 的配置，无法重复添加");
        }

        var configNew = config with { MacroKeys = macroKeys };
        _metaKeyPresetFileService.SaveToConfigDir(configNew);
        return true;
    }

    public bool IsFirstStart()
    {
        ReloadConfigs();
        return Configs.Count == 1 && Configs.ContainsKey("我的收藏");
    }
}
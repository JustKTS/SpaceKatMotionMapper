using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using LanguageExt.Common;
using Serilog;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;

namespace SpaceKatMotionMapper.Services;

public class MetaKeyPresetService
{
    private readonly MetaKeyPresetFileService _metaKeyPresetFileService;
    private readonly ILogger _logger;
    public Dictionary<string, ProgramSpecMetaKeysRecord> Configs { get; private set; } = [];
    
    public MetaKeyPresetService(MetaKeyPresetFileService metaKeyPresetFileService, ILogger logger)
    {
        _metaKeyPresetFileService = metaKeyPresetFileService;
        _logger = logger;
        ReloadConfigs();
    }
    public void ReloadConfigs()
    {
        var either = _metaKeyPresetFileService.LoadConfigs();
        _ = either.Match(config =>
        {
            Configs = config;
        }, ex =>
        {
            _logger.Error(ex, "");
        });
    }

    public Result<bool> AddToFavPreset(string description, List<KeyActionConfig> keyActionConfigs)
    {
        var config = Configs["我的收藏"];
        var macroKeys = config.MacroKeys;
        if (!macroKeys.TryAdd(description, keyActionConfigs))
        {
            return new Result<bool>(new Exception($"已存在名为 \"{description}\" 的配置，无法重复添加"));
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
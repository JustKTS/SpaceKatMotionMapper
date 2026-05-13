using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IMetaKeyPresetService
{
    Dictionary<string, ProgramSpecMetaKeysRecord> Configs { get; }
    void ReloadConfigs();
    Result<bool, Exception> AddToFavPreset(string description, List<KeyActionConfig> keyActionConfigs);
    bool IsFirstStart();
}

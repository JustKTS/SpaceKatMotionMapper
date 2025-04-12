using LanguageExt.Common;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Services.Contract;

public interface IProgramSpecMetaKeyFileService
{
    public Result<bool> SaveToConfigDir(ProgramSpecMetaKeysRecord config);
    public Dictionary<string, ProgramSpecMetaKeysRecord> LoadConfigs();

    public Result<bool> SaveToFile(ProgramSpecMetaKeysRecord config, string filepath);
}
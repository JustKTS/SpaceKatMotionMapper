using CSharpFunctionalExtensions;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Services.Contract;

public interface IMetaKeyPresetFileService
{
    public Result<bool, Exception> SaveToConfigDir(ProgramSpecMetaKeysRecord config);
    public Result<Dictionary<string, ProgramSpecMetaKeysRecord>, Exception> LoadConfigs();

    public Result<bool, Exception> SaveToFile(ProgramSpecMetaKeysRecord config, string filepath);
    public Result<ProgramSpecMetaKeysRecord, Exception> LoadFromFile(string filepath);
}

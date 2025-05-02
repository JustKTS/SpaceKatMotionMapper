using LanguageExt;
using LanguageExt.Common;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Services.Contract;

public interface IMetaKeyPresetFileService
{
    public Result<bool> SaveToConfigDir(ProgramSpecMetaKeysRecord config);
    public Either<Exception, Dictionary<string, ProgramSpecMetaKeysRecord>> LoadConfigs();

    public Result<bool> SaveToFile(ProgramSpecMetaKeysRecord config, string filepath);
    public Either<Exception, ProgramSpecMetaKeysRecord> LoadFromFile(string filepath);
}
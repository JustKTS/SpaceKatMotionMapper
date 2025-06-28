using LanguageExt;
using LanguageExt.Common;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Services.Contract;

public interface IMetaKeyPresetFileService
{
    public Either<Exception, bool> SaveToConfigDir(ProgramSpecMetaKeysRecord config);
    public Either<Exception, Dictionary<string, ProgramSpecMetaKeysRecord>> LoadConfigs();

    public Either<Exception, bool> SaveToFile(ProgramSpecMetaKeysRecord config, string filepath);
    public Either<Exception, ProgramSpecMetaKeysRecord> LoadFromFile(string filepath);
}
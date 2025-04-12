using System.Collections.Generic;
using System.Linq;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services;

namespace SpaceKatMotionMapper.Services;

public class ProgramSpecMetaKeyService
{
    private readonly ProgramSpecMetaKeyFileService _programSpecMetaKeyFileService;
    public Dictionary<string, ProgramSpecMetaKeysRecord> Configs { get; private set; } = [];
    
    public ProgramSpecMetaKeyService(ProgramSpecMetaKeyFileService programSpecMetaKeyFileService)
    {
        _programSpecMetaKeyFileService = programSpecMetaKeyFileService;
        ReloadConfigs();
    }
    public void ReloadConfigs()
    {
        Configs = _programSpecMetaKeyFileService.LoadConfigs();
    }
}
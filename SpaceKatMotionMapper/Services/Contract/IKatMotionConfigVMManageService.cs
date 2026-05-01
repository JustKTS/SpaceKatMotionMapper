using System;
using CSharpFunctionalExtensions;
using SpaceKatMotionMapper.ViewModels;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IKatMotionConfigVMManageService
{
    void RegisterConfig(KatMotionConfigViewModel configVm);
    void RegisterDefaultConfig(KatMotionConfigViewModel configVm);
    Result<KatMotionConfigViewModel, Exception> GetConfig(Guid id);
    Result<KatMotionConfigViewModel, Exception> GetDefaultConfig();
    bool RemoveConfig(Guid id);
}

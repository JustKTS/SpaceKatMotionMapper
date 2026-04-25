using System;
using System.Threading.Tasks;
using LanguageExt;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IOfficialMapperHotKeyService
{
    Task<Either<Exception, bool>> RegisterHotKeyWrapper(bool useCtrl, bool useAlt, bool useShift, KeyCodeWrapper hotKey,
        KatButtonEnum katButtonEnum);

    Either<Exception, bool> UnregisterHotKeyWrapper();
    void RegisterHandle();
    void UnregisterHandle();
}
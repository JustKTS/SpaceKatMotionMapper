using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IOfficialMapperHotKeyService
{
    Task<Result<bool, Exception>> RegisterHotKeyWrapper(bool useCtrl, bool useAlt, bool useShift, KeyCodeWrapper hotKey,
        KatButtonEnum katButtonEnum);
    Result<bool, Exception> UnregisterHotKeyWrapper();
    void RegisterHandle();
    void UnregisterHandle();
}
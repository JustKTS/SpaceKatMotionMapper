using System.Threading.Tasks;
using LanguageExt.Common;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Models;
using WindowsInput;

namespace SpaceKatMotionMapper.Services.Contract;

public interface IOfficialMapperHotKeyService
{
    Task<Result<bool>> RegisterHotKeyWrapper(bool useCtrl, bool useAlt, bool useShift, VirtualKeyCode hotKey,
        KatButtonEnum katButtonEnum);

    Result<bool> UnregisterHotKeyWrapper();
    void RegisterHandle();
    void UnregisterHandle();
}
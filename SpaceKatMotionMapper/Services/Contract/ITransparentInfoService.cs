using System.Threading.Tasks;
using Avalonia.Media;
using SpaceKat.Shared.Models;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

namespace SpaceKatMotionMapper.Services.Contract;

public interface ITransparentInfoService
{
    int AnimationTimeMs { get; set; }
    void SetDisappearTime(int timeMs);
    void DisplayKatMotion(KatMotionWithTimeStamp motion);
    void SetActionInfoMotion(bool isAction, KeyActionConfig[]? actionInfo = null);
    void DisplayOtherInfo(string info);
    void StartAdjustInfoWindow();
    void StopAdjustInfoWindow();
    Task SaveConfigsAsync(int x, int y, double width, double height, Color backgroundColor,
        Color fontColor, double fontSize, int disappearTimeMs, int animationTimeMs);
    Task<TransparentInfoWindowConfig?> LoadConfigs();
    Task UpdateTimeConfigs(int disappearTimeMs, int animationTimeMs);
}

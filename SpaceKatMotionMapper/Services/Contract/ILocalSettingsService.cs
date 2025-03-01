using System.Threading.Tasks;

namespace SpaceKatMotionMapper.Services.Contract;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
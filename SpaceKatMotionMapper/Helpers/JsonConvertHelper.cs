using System.Text.Json;
using System.Threading.Tasks;

namespace SpaceKatMotionMapper.Helpers;

public static class JsonConvertHelper
{
    public static async Task<T?> ToObjectAsync<T>(string value) =>
        await Task.Run<T>(() =>
        {
#pragma warning disable CS8603 // 可能返回 null 引用。
            return JsonSerializer.Deserialize<T>(value);
#pragma warning restore CS8603 // 可能返回 null 引用。
        });

    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run(() => JsonSerializer.Serialize(value));
    }
}
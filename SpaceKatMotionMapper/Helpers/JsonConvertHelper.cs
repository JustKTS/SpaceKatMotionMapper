using System.Text.Json;
using System.Threading.Tasks;

namespace SpaceKatMotionMapper.Helpers;

public static class JsonConvertHelper
{
    public static async Task<T?> ToObjectAsync<T>(string value) =>
        await Task.Run<T>(() =>
        {
#pragma warning disable CS8603 // 可能返回 null 引用。
            return JsonSerializer.Deserialize(value, JsonSgOption.GetTypeInfo<T>());
#pragma warning restore CS8603 // 可能返回 null 引用。
        });
}
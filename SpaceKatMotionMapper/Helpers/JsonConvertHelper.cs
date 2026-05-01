using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpaceKatMotionMapper.Helpers;

public static class JsonConvertHelper
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access",
        Justification = "All serializable types are registered in JsonSgOption source generator contexts.")]
    public static async Task<T?> ToObjectAsync<T>(string value) =>
        await Task.Run<T>(() =>
        {
#pragma warning disable CS8603 // 可能返回 null 引用。
            return JsonSerializer.Deserialize<T>(value,JsonSgOption.Default);
#pragma warning restore CS8603 // 可能返回 null 引用。
        });

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access",
        Justification = "All serializable types are registered in JsonSgOption source generator contexts.")]
    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run(() => JsonSerializer.Serialize(value, JsonSgOption.Default));
    }
}
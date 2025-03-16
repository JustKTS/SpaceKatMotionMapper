using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

[JsonConverter(typeof(JsonStringEnumConverter<KatButtonEnum>))]
[EnumExtensions]
public enum KatButtonEnum
{
    [Display(Name = "无")]
    None,
    [Display(Name = "左")]
    Left,
    [Display(Name = "右")]
    Right
}

[JsonSourceGenerationOptions(WriteIndented = true ,UseStringEnumConverter = true)]
[JsonSerializable(typeof(KatButtonEnum))]
internal partial class KatButtonEnumJsonSgContext : JsonSerializerContext
{
}
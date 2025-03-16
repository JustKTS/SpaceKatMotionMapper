using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

[JsonConverter(typeof(JsonStringEnumConverter<PressModeEnum>))]
[EnumExtensions]
public enum PressModeEnum
{
    [Display(Name="无操作")]
    None,
    [Display(Name="单击")]
    Click,
    [Display(Name="双击")]
    DoubleClick,
    [Display(Name="按住")]
    Press,
    [Display(Name="松开")]
    Release,
}

[JsonSourceGenerationOptions(WriteIndented = true ,UseStringEnumConverter = true)]
[JsonSerializable(typeof(PressModeEnum))]
internal partial class PressModeEnumJsonSgContext : JsonSerializerContext
{
}
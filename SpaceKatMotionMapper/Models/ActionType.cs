using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

[JsonConverter(typeof(JsonStringEnumConverter<ActionType>))]
[EnumExtensions]
public enum ActionType
{
    [Display(Name="键盘")]
    KeyBoard,
    [Display(Name="鼠标")]
    Mouse,
    [Display(Name="延时")]
    Delay
}

[JsonSourceGenerationOptions(WriteIndented = true ,UseStringEnumConverter = true)]
[JsonSerializable(typeof(ActionType))]
internal partial class ActionTypeJsonSgContext : JsonSerializerContext
{
}
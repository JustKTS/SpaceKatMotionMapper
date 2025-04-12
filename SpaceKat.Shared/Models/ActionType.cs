using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKat.Shared.Models;

[JsonConverter(typeof(JsonStringEnumConverter<ActionType>))]
[EnumExtensions]
public enum ActionType
{
    [Display(Name="键盘")]
    KeyBoard,
    [Display(Name="鼠标")]
    Mouse,
    [Display(Name="延时")]
    Delay,
    [Display(Name = "未定义")]
    None,
}

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ActionType))]
public partial class ActionTypeJsonSgContext : JsonSerializerContext
{
}
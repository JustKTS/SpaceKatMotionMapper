using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

[EnumExtensions]
public enum MouseButtonEnum
{
    [Display(Name="无操作")] None = 0,
    [Display(Name="左键")] LButton = 1,
    [Display(Name="右键")] RButton = 2,
    [Display(Name="中键")] MButton = 3,
    [Display(Name="滚轮上滑")] ScrollUp = 4,
    [Display(Name="滚轮下滑")] ScrollDown = 5,
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MouseButtonEnum))]
[JsonSerializable(typeof(int))]

internal partial class MouseButtonEnumJsonSgContext : JsonSerializerContext
{
}
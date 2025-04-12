using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKat.Shared.Models;

[EnumExtensions]
public enum MouseButtonEnum
{
    [Display(Name="无操作")] None = 0,
    [Display(Name="左键")] LButton = 1,
    [Display(Name="右键")] RButton = 2,
    [Display(Name="中键")] MButton = 3,
    [Display(Name="滚轮上滑")] ScrollUp = 4,
    [Display(Name="滚轮下滑")] ScrollDown = 5,
    [Display(Name = "横滚轮左滑")] ScrollLeft = 6,
    [Display(Name = "横滚轮右滑")] ScrollRight = 7
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MouseButtonEnum))]
[JsonSerializable(typeof(int))]

public partial class MouseButtonEnumJsonSgContext : JsonSerializerContext
{
}
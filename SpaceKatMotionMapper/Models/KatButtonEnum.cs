using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

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
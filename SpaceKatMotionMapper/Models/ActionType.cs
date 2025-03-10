using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

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
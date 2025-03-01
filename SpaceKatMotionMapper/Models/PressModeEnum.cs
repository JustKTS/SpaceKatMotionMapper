using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;


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

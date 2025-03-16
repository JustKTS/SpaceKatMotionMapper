using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKatHIDWrapper.Models;

[EnumExtensions]
public enum KatMotionEnum
{
    [Display(Name = "无动作")] Stable = 0,
    [Display(Name = "右推")] TranslationXPositive = 1,
    [Display(Name = "左推")] TranslationXNegative = 2,
    [Display(Name = "前推")] TranslationYPositive = 3,
    [Display(Name = "后拉")] TranslationYNegative = 4,
    [Display(Name = "上提")] TranslationZPositive = 5,
    [Display(Name = "下按")] TranslationZNegative = 6,
    [Display(Name = "横滚-右倾")] RollPositive = 7,
    [Display(Name = "横滚-左倾")] RollNegative = 8,
    [Display(Name = "俯仰-俯")] PitchPositive = 9,
    [Display(Name = "俯仰-仰")] PitchNegative = 10,
    [Display(Name = "平转-右旋")] YawPositive = 11,
    [Display(Name = "平转-左旋")] YawNegative = 12,
    [Display(Name = "未定义")] Null = 999
}

[EnumExtensions]
public enum KatPressModeEnum
{
    [Display(Name = "短触")] Short = 0,
    [Display(Name = "长推保持")] LongReach = 1,
    [Display(Name = "长推结束")] LongDown = 2,
    Null = 999
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatMotionEnum))]
[JsonSerializable(typeof(int))]
public partial class KatMotionEnumJsonSgContext : JsonSerializerContext
{
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(KatPressModeEnum))]
[JsonSerializable(typeof(int))]
public partial class KatPressModeEnumJsonSgContext : JsonSerializerContext
{
}
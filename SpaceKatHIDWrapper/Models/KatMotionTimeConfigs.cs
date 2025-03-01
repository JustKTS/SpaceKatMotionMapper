namespace SpaceKatHIDWrapper.Models;

public record KatMotionTimeConfigs(Dictionary<KatMotionEnum, KatTriggerTimesConfig> Configs)
{
    public KatMotionTimeConfigs(bool isDefault) : this(new Dictionary<KatMotionEnum, KatTriggerTimesConfig>())
    {
        if (isDefault)
        {
            Configs = MotionTimeConfigInitHelper.GeneDefault();
        }
    }
    
    // 为Json反序列化使用
    public KatMotionTimeConfigs():this(new Dictionary<KatMotionEnum, KatTriggerTimesConfig>()){}
}

public static class MotionTimeConfigInitHelper
{
    public static Dictionary<KatMotionEnum, KatTriggerTimesConfig> GeneDefault()
    {
        return KatMotionEnumExtensions.GetValues().Where(e => e is not (KatMotionEnum.Null or KatMotionEnum.Stable))
            .ToDictionary(motion => motion, _ => new KatTriggerTimesConfig());
    }
}
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKat.Shared.Models;

namespace SpaceKatMotionMapper.Helpers;

public static class JsonSgOption
{
    public static JsonSerializerOptions Default => new()
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
            JsonDefaultSgContext.Default,
            ActionTypeJsonSgContext.Default,
            KatMotionJsonSgContext.Default,
            KatDeadZoneConfigJsonSgContext.Default,
            KatMotionEnumJsonSgContext.Default,
            KatPressModeEnumJsonSgContext.Default,
            KatMotionTimeConfigsJsonSgContext.Default,
            KatTriggerTimesConfigJsonSgContext.Default,
            ActionTypeJsonSgContext.Default,
            DelayActionConfigJsonSgContext.Default,
            HotKeyCodeJsonSgContext.Default,
            HotKeyRecordJsonSgContext.Default,
            KatMotionConfigGroupJsonSgContext.Default,
            KatMotionInfoJsonSgContext.Default,
            KatMotionConfigJsonSgContext.Default,
            KeyActionConfigJsonSgContext.Default,
            KatButtonEnumJsonSgContext.Default,
            KatDataWithInfoJsonSgContext.Default,
            KeyBoardActionConfigJsonSgContext.Default,
            MouseActionConfigJsonSgContext.Default,
            MouseButtonEnumJsonSgContext.Default,
            PressModeEnumJsonSgContext.Default,
            TransparentInfoWindowConfigJsonSgContext.Default,
            CombinationKeysRecordJsonSgContext.Default
            ),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.CjkUnifiedIdeographs, UnicodeRanges.BasicLatin)
    };
}
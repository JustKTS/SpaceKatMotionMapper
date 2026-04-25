using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;
using SpaceKat.Shared.Helpers;
using SpaceKat.Shared.Models;
#if LINUX
using LinuxHelpers.Services.Window.Lswt;
#endif

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
            SpaceMouseXmlKeyEnumJsonSgContext.Default,
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
#if LINUX
            , LinuxHelpers.Services.Window.Lswt.LswtJsonContext.Default
#endif
            ),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.CjkUnifiedIdeographs, UnicodeRanges.BasicLatin)
    };
}
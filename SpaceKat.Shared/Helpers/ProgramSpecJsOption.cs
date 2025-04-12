using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using SpaceKat.Shared.Models;

namespace SpaceKat.Shared.Helpers;

public static class ProgramSpecJsOption
{
    public static JsonSerializerOptions Default => new()
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
            ActionTypeJsonSgContext.Default,
            ActionTypeJsonSgContext.Default,
            DelayActionConfigJsonSgContext.Default,
            KeyActionConfigJsonSgContext.Default,
            KeyBoardActionConfigJsonSgContext.Default,
            MouseActionConfigJsonSgContext.Default,
            MouseButtonEnumJsonSgContext.Default,
            PressModeEnumJsonSgContext.Default,
            CombinationKeysRecordJsonSgContext.Default,
            ProgramSpecMetaKeysRecordJsonSgContext.Default
        ),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.CjkUnifiedIdeographs, UnicodeRanges.BasicLatin)
    };
}

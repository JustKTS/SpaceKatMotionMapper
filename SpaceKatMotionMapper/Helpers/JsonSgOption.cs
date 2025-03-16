﻿using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SpaceKatHIDWrapper.Models;
using SpaceKatMotionMapper.Models;

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
            TransparentInfoWindowConfigJsonSgContext.Default
            ),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}
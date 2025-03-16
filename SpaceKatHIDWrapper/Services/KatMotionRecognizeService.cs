using CommunityToolkit.Mvvm.ComponentModel;
using SpaceKatHIDWrapper.DeviceWrappers;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.Services;

public partial class KatMotionRecognizeService : ObservableObject
{
    private bool IsConnected => _deviceDataWrapper.IsConnected;
    public event EventHandler<bool>? ConnectionChanged;
    private readonly KatDeadZoneConfig _deadZoneConfig = new();

    private readonly Dictionary<KatMotionEnum, KatTriggerTimesConfig> _motionTimeConfigs =
        MotionTimeConfigInitHelper.GeneDefault();

    public event EventHandler<KatMotionWithTimeStamp>? DataReceived;

    private DateTimeOffset _pressTime = DateTimeOffset.Now;
    private bool _pressDone;

    [ObservableProperty] private KatDeviceData _katDeviceData = new();

    [ObservableProperty] private KatMotionWithTimeStamp _currentKatMotion = new(KatMotionEnum.Stable, KatPressModeEnum.Short, 0);

    private readonly IDeviceDataWrapper _deviceDataWrapper;
    public KatMotionRecognizeService(IDeviceDataWrapper deviceDataWrapper)
    {
        _deviceDataWrapper = deviceDataWrapper;
        _deviceDataWrapper.ConnectionChanged += (_, isConnected)=>
        {
            ConnectionChanged?.Invoke(this, isConnected);
        };
    }
    
    partial void OnCurrentKatMotionChanged(KatMotionWithTimeStamp value)
    {
        DataReceived?.Invoke(this, value);
    }

    public ManualResetEventSlim ExitEvent { get; } = new(false);


    public void SetDeadZoneByAxis(MotionAxis axis, bool isUpper, double value)
    {
        if (isUpper) _deadZoneConfig.Upper[(int)axis] = value;
        else _deadZoneConfig.Lower[(int)axis] = value;
    }

    public void SetDeadZone(double[] uppers, double[] lowers)
    {
        if (uppers.Length != 6) return;
        for (var i = 0; i < uppers.Length; i++)
        {
            _deadZoneConfig.Upper[i] = uppers[i];
        }

        if (lowers.Length != 6) return;
        for (var i = 0; i < lowers.Length; i++)
        {
            _deadZoneConfig.Lower[i] = lowers[i];
        }
    }

    public void UpdateMotionTimeConfigs(KatMotionTimeConfigs configs)
    {
        _motionTimeConfigs.Clear();
        foreach (var pair in configs.Configs)
        {
            _motionTimeConfigs.Add(pair.Key, pair.Value);
        }
    }

    public async Task StartRecognizeMotionAsync()
    {
        var lastMotion = KatMotionEnum.Stable;
        var lastLongReachTriggerTime = DateTimeOffset.Now;
        var longReachTriggerCount = 1;

        while (!ExitEvent.IsSet && IsConnected)
        {
            var singleRet = RecognizeSingleAction();
            if (singleRet == KatMotionEnum.Null) continue;
            if (singleRet != KatMotionEnum.Stable)
            {
                lastMotion = singleRet;

                if (_pressDone)
                {
                    _pressTime = DateTimeOffset.Now;
                    _pressDone = false;
                }
                else
                {
                    // 长按检测
                    if (DateTimeOffset.Now - _pressTime <=
                        TimeSpan.FromMilliseconds(_motionTimeConfigs[lastMotion].LongReachTimeoutMs)) continue;

                    // 长按触发间隔调整, 以最短时间为基础，随着动作行程逐渐增加
                    var longReachRepeatMotionTimeSpan = DateTimeOffset.Now - lastLongReachTriggerTime;

                    var motionDistance = lastMotion switch
                    {
                        KatMotionEnum.TranslationXPositive or KatMotionEnum.TranslationXNegative => KatDeviceData
                            .Translation[0],
                        KatMotionEnum.TranslationYPositive or KatMotionEnum.TranslationYNegative => KatDeviceData
                            .Translation[1],
                        KatMotionEnum.TranslationZPositive or KatMotionEnum.TranslationZNegative => KatDeviceData
                            .Translation[2],
                        KatMotionEnum.RollPositive or KatMotionEnum.RollNegative => KatDeviceData.Rotation[0],
                        KatMotionEnum.PitchPositive or KatMotionEnum.PitchNegative => KatDeviceData.Rotation[1],
                        KatMotionEnum.YawPositive or KatMotionEnum.YawNegative => KatDeviceData.Rotation[2],
                        _ => 0
                    };
                    var motionDistanceAbs = Math.Abs(motionDistance);

                    var longReachRepeatTriggerTimeSpan =
                        _motionTimeConfigs[lastMotion].LongReachRepeatScaleFactor switch
                        {
                            0 => TimeSpan.FromMilliseconds(
                                _motionTimeConfigs[lastMotion].LongReachRepeatTimeSpanMs),
                            _ =>
                                TimeSpan.FromMilliseconds(
                                    _motionTimeConfigs[lastMotion].LongReachRepeatTimeSpanMs /
                                    (_motionTimeConfigs[lastMotion].LongReachRepeatScaleFactor * motionDistanceAbs))
                        };
                    if (longReachRepeatMotionTimeSpan < longReachRepeatTriggerTimeSpan)
                    {
                        continue;
                    }

                    CurrentKatMotion = new KatMotionWithTimeStamp(lastMotion, KatPressModeEnum.LongReach, longReachTriggerCount);
                    longReachTriggerCount += 1;
                    lastLongReachTriggerTime = DateTimeOffset.Now;
                }
            }
            else
            {
                if (lastMotion == KatMotionEnum.Stable) continue;
                if (!_pressDone)
                {
                    if ((DateTimeOffset.Now - _pressTime) >
                        TimeSpan.FromMilliseconds(_motionTimeConfigs[lastMotion].LongReachTimeoutMs))
                    {
                        longReachTriggerCount = 1;
                        CurrentKatMotion = new KatMotionWithTimeStamp(lastMotion, KatPressModeEnum.LongDown, 1);
                    }
                    else
                    {
                        longReachTriggerCount = 1;
                        CurrentKatMotion = await RecognizeMultiShortMotionAsync(lastMotion);
                    }
                }

                _pressDone = true;
            }
        }
    }


    private async Task<KatMotionWithTimeStamp> RecognizeMultiShortMotionAsync(KatMotionEnum lastMotion = KatMotionEnum.Stable)
    {
        var startTime = DateTimeOffset.Now;
        var waitTime = TimeSpan.Zero;
        var motionList = new List<KatMotionEnum>();
        var recRet = lastMotion;
        var timeSpan = TimeSpan.FromMilliseconds(_motionTimeConfigs[lastMotion].ShortRepeatToleranceMs);
        while (waitTime <= timeSpan)
        {
            var singleRet1 = RecognizeSingleAction();
            if (singleRet1 == KatMotionEnum.Null) break;
            if (singleRet1 != KatMotionEnum.Stable)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20));
                var singleRet2 = RecognizeSingleAction();

                if (singleRet1 == singleRet2)
                {
                    while (RecognizeSingleAction() != KatMotionEnum.Stable) ;
                    motionList.Add(singleRet2);
                    startTime = DateTimeOffset.Now;
                    recRet = singleRet2;
                }
            }

            waitTime = DateTimeOffset.Now - startTime;
        }

        return new KatMotionWithTimeStamp(recRet, KatPressModeEnum.Short, motionList.Count + 1);
    }

    private KatMotionEnum RecognizeSingleAction()
    {
        var data = _deviceDataWrapper.Read();
        if (data is null) return KatMotionEnum.Null;

        KatDeviceData = data;

        var rotation = data.Rotation;
        var rotationAbs = rotation.Select(Math.Abs).ToArray();
        var rotationMainIndex = Array.IndexOf(rotationAbs, rotationAbs.Max());

        if (rotation[rotationMainIndex] >= _deadZoneConfig.Upper[rotationMainIndex + 3] ||
            rotation[rotationMainIndex] <= _deadZoneConfig.Lower[rotationMainIndex + 3])
        {
            if (rotation[rotationMainIndex] < 0)
            {
                return rotationMainIndex switch
                {
                    0 => KatMotionEnum.RollNegative,
                    1 => KatMotionEnum.PitchNegative,
                    2 => KatMotionEnum.YawNegative,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return rotationMainIndex switch
            {
                0 => KatMotionEnum.RollPositive,
                1 => KatMotionEnum.PitchPositive,
                2 => KatMotionEnum.YawPositive,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        var translation = data.Translation;

        var translationAbs = data.Translation.Select(Math.Abs).ToArray();
        var translationMainIndex = Array.IndexOf(translationAbs, translationAbs.Max());

        if (!(translationAbs[translationMainIndex] >= _deadZoneConfig.Upper[translationMainIndex] ||
              translationAbs[translationMainIndex] <= _deadZoneConfig.Lower[translationMainIndex]))
            return KatMotionEnum.Stable;

        if (translation[translationMainIndex] < 0)
        {
            return translationMainIndex switch
            {
                0 => KatMotionEnum.TranslationXNegative,
                1 => KatMotionEnum.TranslationYNegative,
                2 => KatMotionEnum.TranslationZNegative,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return translationMainIndex switch
        {
            0 => KatMotionEnum.TranslationXPositive,
            1 => KatMotionEnum.TranslationYPositive,
            2 => KatMotionEnum.TranslationZPositive,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
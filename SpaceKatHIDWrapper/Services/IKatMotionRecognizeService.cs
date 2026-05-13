using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.Services;

public interface IKatMotionRecognizeService
{
    event EventHandler<Result<bool, Exception>>? ConnectionChanged;
    event EventHandler<KatMotionWithTimeStamp>? DataReceived;
    ManualResetEventSlim ExitEvent { get; }
    KatMotionWithTimeStamp CurrentKatMotion { get; }
    KatDeviceData KatDeviceData { get; }
    void SetDeadZone(double[] uppers, double[] lowers, bool[]? axesInverse);
    void UpdateMotionTimeConfigs(KatMotionTimeConfigs configs);
    Task StartRecognizeMotionAsync();
}

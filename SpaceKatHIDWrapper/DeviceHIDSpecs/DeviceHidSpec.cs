using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

public record DeviceHidSpec(
    string Name,
    int[] HidId,
    FrozenDictionary<MotionAxis, AxisSpecs> AxesMappings,
    ReadOnlyCollection<ButtonSpecs> ButtonMappings,
    double AxisScale);
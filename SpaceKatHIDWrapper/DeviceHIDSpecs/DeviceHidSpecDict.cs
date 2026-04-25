using System.Collections.Frozen;
using SpaceKatHIDWrapper.Models;

namespace SpaceKatHIDWrapper.DeviceHIDSpecs;

public static class DeviceHidSpecDict
{
    public static readonly FrozenDictionary<string, DeviceHidSpec> HidIds = new Dictionary<string, DeviceHidSpec>
    {
        ["Mini"] = new("SpaceKat V3 Mini", [0x256F, 0xC635], new Dictionary<MotionAxis, AxisSpecs>()
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1),
            [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary(), new List<ButtonSpecs>
        {
            new(3, 1, 0), // LEFT
            new(3, 1, 1)  // RIGHT
        }.AsReadOnly(), 350.0),
        ["Mini_V1.3"] = new("SpaceKat V3 Mini", [0x256F, 0xC63A], new Dictionary<MotionAxis, AxisSpecs>()
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1),
            [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary(), new List<ButtonSpecs>
        {
            new(3, 1, 0), // LEFT
            new(3, 1, 1)  // RIGHT
        }.AsReadOnly(), 350.0),
        ["MiniE"] = new("SpaceKat V3 MiniE", [0x256F, 0xC638], new Dictionary<MotionAxis, AxisSpecs>()
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1),
            [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary(), new List<ButtonSpecs>
        {
            new(3, 4, 1), // LEFT1
            new(3, 1, 2), // LEFT2
            new(3, 1, 5), // LEFT3
            new(3, 1, 1), // RIGHT1
            new(3, 1, 4), // RIGHT2
            new(3, 2, 0)  // RIGHT3
        }.AsReadOnly(), 350.0),// TODO: Pro与MiniE硬件码相同
        ["MiniEWired"] = new("SpaceKat V3 MiniE Wired", [0x256F, 0xC631], new Dictionary<MotionAxis, AxisSpecs>()
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1),
            [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary(), new List<ButtonSpecs>
        {
            new(3, 4, 1), // LEFT1
            new(3, 1, 2), // LEFT2
            new(3, 1, 5), // LEFT3
            new(3, 1, 1), // RIGHT1
            new(3, 1, 4), // RIGHT2
            new(3, 2, 0)  // RIGHT3
        }.AsReadOnly(), 350.0),// TODO: Pro与MiniE硬件码相同
        ["ProDesigner"] = new("SpaceKatV3 Pro Designer", [0x256F, 0xC633], new Dictionary<MotionAxis, AxisSpecs>()
        {
            [MotionAxis.X] = new(1, 1, 2, 1),
            [MotionAxis.Y] = new(1, 3, 4, -1),
            [MotionAxis.Z] = new(1, 5, 6, -1),
            [MotionAxis.Pitch] = new(1, 7, 8, -1),
            [MotionAxis.Roll] = new(1, 9, 10, -1),
            [MotionAxis.Yaw] = new(1, 11, 12, 1),
        }.ToFrozenDictionary(), new List<ButtonSpecs>
        {
            new(3, 4, 1), // LEFT1
            new(3, 1, 2), // LEFT2
            new(3, 1, 5), // LEFT3
            new(3, 1, 1), // RIGHT1
            new(3, 4, 2), // RIGHT2
            new(3, 2, 0)  // RIGHT3
        }.AsReadOnly(), 350.0), // TODO: 按钮定义没有修改，仅可用使用旋钮部分
    }.ToFrozenDictionary();
}
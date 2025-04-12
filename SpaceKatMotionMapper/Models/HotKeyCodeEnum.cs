using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace SpaceKatMotionMapper.Models;

[EnumExtensions]
public enum SpaceMouseXmlKeyEnum:uint
{
    A = 0x4,
    B = 0x5,
    C = 0x6,
    D = 0x7,
    E = 0x8,
    F = 0x9,
    G = 0xA,
    H = 0xB,
    I = 0xC,
    J = 0xD,
    K = 0xE,
    L = 0xF,
    M = 0x10,
    N = 0x11,
    O = 0x12,
    P = 0x13,
    Q = 0x14,
    R = 0x15,
    S = 0x16,
    T = 0x17,
    U = 0x18,
    V = 0x19,
    W = 0x1A,
    X = 0x1B,
    Y = 0x1C,
    Z = 0x1D
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SpaceMouseXmlKeyEnum))]
[JsonSerializable(typeof(uint))]
internal partial class HotKeyCodeJsonSgContext : JsonSerializerContext
{
}
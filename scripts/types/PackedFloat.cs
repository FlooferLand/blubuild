using System;
using Godot;

namespace Project;

public struct PackedFloat {
    public ushort F;

    const float Range = 1000f;

    public static PackedFloat Encode(float f) =>
        new() { F = (ushort) Math.Clamp((f + Range) / (Range * 2f) * ushort.MaxValue, 0, ushort.MaxValue) };

    public float Decode() =>
        (F / (float) ushort.MaxValue) * (Range * 2f) - Range;
}
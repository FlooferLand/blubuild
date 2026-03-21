using System;
using Godot;

namespace Project;

/// Vector2 but packed into 2 shorts, mainly for networking
public struct PackedVec2 {
    public PackedFloat X, Y;

    public static PackedVec2 Encode(Vector2 vec) =>
        new() {
            X = PackedFloat.Encode(vec.X),
            Y = PackedFloat.Encode(vec.Y)
        };

    public Vector2 Decode() =>
        new() {
            X = X.Decode(),
            Y = Y.Decode()
        };
}
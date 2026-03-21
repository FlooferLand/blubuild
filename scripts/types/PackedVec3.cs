using System;
using Godot;

namespace Project;

/// Vector3 but packed into 3 shorts, mainly for networking
public struct PackedVec3 {
    public PackedFloat X, Y, Z;

    public static PackedVec3 Encode(Vector3 vec) =>
        new() {
            X = PackedFloat.Encode(vec.X),
            Y = PackedFloat.Encode(vec.Y),
            Z = PackedFloat.Encode(vec.Z)
        };

    public Vector3 Decode() =>
        new() {
            X = X.Decode(),
            Y = Y.Decode(),
            Z = Z.Decode()
        };
}
using System;

namespace Project;

public readonly record struct Bit(short Id) : IComparable<Bit>, IComparable<short> {
    public Drawer ToDrawer() => Drawer.FromBit(Id);

    public int CompareTo(Bit other) => Id.CompareTo(other.Id);
    public int CompareTo(short other) => Id.CompareTo(other);
    public override string ToString() => Id.ToString();
    public string FormatDrawered() => Drawer.FormatBit(this);

    // Math
    public static Bit operator -(Bit left, Bit right) => new((short)(left.Id - right.Id));
    public static Bit operator +(Bit left, Bit right) => new((short)(left.Id + right.Id));
    public static Bit operator -(Bit left, short right) => new((short)(left.Id - right));
    public static Bit operator +(Bit left, short right) => new((short)(left.Id + right));

    // Comparison
    public static bool operator <(Bit left, short right) => left.CompareTo(right) < 0;
    public static bool operator >(Bit left, short right) => left.CompareTo(right) > 0;
    public static bool operator <=(Bit left, short right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Bit left, short right) => left.CompareTo(right) >= 0;

    public static implicit operator Bit(short id) => new(id);
}

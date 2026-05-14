using Godot;

namespace Project;

/// A bit and the chart it's using
public readonly record struct MappedBit(StringName Chart, Bit Bit) {
    public Drawer ToDrawer() => Drawer.FromBit(Bit);
    public override string ToString() => $"{Chart}:{Bit}";
    public string FormatDrawered() => Drawer.FormatBit(this);
}

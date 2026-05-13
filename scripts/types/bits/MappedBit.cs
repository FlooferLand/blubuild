namespace Project;

public readonly record struct MappedBit(string Chart, Bit Bit) {
    public Drawer ToDrawer() => Drawer.FromBit(Bit);
    public override string ToString() => $"{Chart}:{Bit}";
    public string FormatDrawered() => Drawer.FormatBit(this);
}

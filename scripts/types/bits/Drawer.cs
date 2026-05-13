namespace Project;

public readonly record struct Drawer(int Index, string Mark, string English) {
    public static readonly Drawer Top = new(0, "td", "Top drawer");
    public static readonly Drawer Bottom = new(1, "bd", "Bottom drawer");

    public const short Next = 150;
    public static Drawer FromBit(Bit bit) => bit < Next ? Top : Bottom;
    public static string FormatBit(Bit bit) => $"{(bit > Next ? bit - Next : bit)} {FromBit(bit).Mark}";
    public static string FormatBit(MappedBit mapped) => $"{(mapped.Bit > Next ? mapped.Bit - Next : mapped)} {FromBit(mapped.Bit).Mark}";
}
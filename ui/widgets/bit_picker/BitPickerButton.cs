using Godot;

namespace Project;

public partial class BitPickerButton : Button {
	[ExportGroup("Local")]
	[Export] public required BitPicker Picker;

	public override void _Ready() {
		Picker.Selected += () => SetBit(Picker.SelectedBit);
		Pressed += () => Picker.PopupBitPicker();
	}

	public void SetBit(MappedBit bit) {
		Text = $"{bit}";
		TooltipText = $"{bit.FormatDrawered()}";
		Picker.SelectedBit = bit;
	}
}

using Godot;

namespace Project;

public partial class BitPickerButton : Button {
	[ExportGroup("Local")]
	[Export] public required BitPicker Picker;

	public override void _Ready() {
		Picker.Selected += () => {
			Text = $"{Picker.SelectedBit}";
			TooltipText = $"{Picker.SelectedBit.FormatDrawered()}";
		};
		Pressed += () => Picker.PopupBitPicker();
	}
}

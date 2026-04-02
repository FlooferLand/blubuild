using Godot;

namespace Project;

public partial class SettingsWidget : Control {
	[ExportGroup("Local")]
	[Export] public required Environment Environment;
	[Export] public required Button BadGraphicsButton;
	[Export] public required Button GoodGraphicsButton;

	public override void _Ready() {
		BadGraphicsButton.Pressed += () => {
			Environment.SsaoEnabled = false;
			Environment.SsrEnabled = false;
		};
		GoodGraphicsButton.Pressed += () => {
			Environment.SsaoEnabled = true;
			Environment.SsrEnabled = true;
		};
	}
}

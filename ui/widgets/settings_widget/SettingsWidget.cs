using Godot;
using GodotUtils;

namespace Project;

// TODO: Completely rework SettingsWidget and the GraphicsLoader

public partial class SettingsWidget : Control {
	[ExportGroup("Local")]
	[Export] public required Button ShitGraphicsButton;
	[Export] public required Button BadGraphicsButton;
	[Export] public required Button GoodGraphicsButton;

	public override void _Ready() {
		var viewport = (Engine.GetMainLoop() as SceneTree)?.Root?.GetViewport();
		if (viewport == null) {
			Log.Error($"Failed to load the main viewport for {nameof(SettingsWidget)}");
			return;
		}

		// Signals
		ShitGraphicsButton.Pressed += () => {
			GraphicsAutoload.SetPreset(GraphicsAutoload.Presets.Shit);
		};
		BadGraphicsButton.Pressed += () => {
			GraphicsAutoload.SetPreset(GraphicsAutoload.Presets.Bad);
		};
		GoodGraphicsButton.Pressed += () => {
			GraphicsAutoload.SetPreset(GraphicsAutoload.Presets.Good);
		};
	}
}

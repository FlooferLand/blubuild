namespace Project;
using Godot;

public partial class Autoload : Node {
	public static Player Player;

	public override void _Ready() {
		if (OS.IsDebugBuild()) {
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
			Engine.MaxFps = 100;
		}
	}

	public string GetWindowTitle() {
		return $"Blubuild v0.0.1-DEV";
	}
	
	public override void _Process(double delta) {
		GetWindow().Title = $"{GetWindowTitle()}  |  {Engine.GetFramesPerSecond()} FPS";
	}
}

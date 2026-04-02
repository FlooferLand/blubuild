using Godot;

namespace Project;

public partial class PauseScreen : AbstractScreen {
	[ExportGroup("Local")]
	[Export] public required AnimationPlayer AnimPlayer;
	[Export] public required AudioStreamPlayer PauseMusic;
	[Export] public required Button ResumeButton;
	[Export] public required Button QuitFullyButton;

	float pauseMusicVolume = 0.0f;

	public override void _EnterTree() {
		base._EnterTree();
		pauseMusicVolume = PauseMusic.VolumeLinear;
		ResumeButton.Pressed += () => SetPaused(false);
		QuitFullyButton.Pressed += () => GetTree().Quit();

		// Reset
		Visible = false;
		MouseFilter = MouseFilterEnum.Ignore;
		PauseMusic.VolumeLinear = 0f;
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (Input.IsActionJustPressed("pause")) {
			ScreenToggle();
		}
		PauseMusic.VolumeLinear = Mathf.Lerp(PauseMusic.VolumeLinear, ScreenIsActive() ? pauseMusicVolume : 0.0f, 0.2f * (float) delta);
	}

	void SetPaused(bool paused) {
		GetTree().Paused = paused;
		Input.MouseMode = paused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		switch (paused) {
			case true:
				AnimPlayer.Play("pop_in");
				break;
			case false:
				AnimPlayer.Play("pop_out");
				PauseMusic.VolumeLinear = 0f;
				break;
		}
	}

	protected override void OnScreenOpen() => SetPaused(true);
	protected override void OnScreenRemove() => SetPaused(false);
}

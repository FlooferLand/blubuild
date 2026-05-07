using Godot;

namespace Project.CharacterEditing;

public partial class CharacterEditor : Node3D {
	[Signal] public delegate void CharacterLoadedEventHandler();
	[Export] public required Transition SceneTransition;
	[Export] public required InitCanvas InitCanvas;
	[Export] public required EditCanvas EditCanvas;

	public CharacterFile? File = null;

	public override void _Ready() {
		InitCanvas.Visible = true;
		EditCanvas.Visible = false;
		SceneTransition.FadeOut();
	}

	public async void SaveCharacter() {
		if (File == null) return;
		string? error = await File.Write(progress: report => EditCanvas.SaveProgressBar.Value = (float)(report.PercentComplete ?? 0.0));
		if (error != null) EditCanvas.SaveError.Text = error;
	}

	public void LoadCharacter(CharacterFile file) {
		File = file;
		EmitSignalCharacterLoaded();
		SceneTransition.FadeIn(finished: () => {
			InitCanvas.Visible = false;
			EditCanvas.Visible = true;
			SceneTransition.FadeOut();
		});
	}
}

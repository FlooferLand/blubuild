using System.Threading.Tasks;
using Godot;
using SharpCompress.Common;

namespace Project.CharacterEditing;

public partial class CharacterEditor : Node3D {
	[Signal] public delegate void CharacterLoadedEventHandler();
	[Export] public required Node3D BotHolder;
	[Export] public required Transition SceneTransition;
	[Export] public required InitCanvas InitCanvas;
	[Export] public required EditCanvas EditCanvas;

	public CharacterFile? File = null;

	public override void _Ready() {
		InitCanvas.Visible = true;
		EditCanvas.Visible = false;
		SceneTransition.FadeOut();
	}

	public void SetError(string? error) {
		if (InitCanvas.Visible) InitCanvas.SetError(error);
		if (EditCanvas.Visible) EditCanvas.SetError(error);
	}

	public void HandleProgress(ProgressReport report) {
		EditCanvas.SaveProgressBar.Value = (float)(report.PercentComplete ?? 0.0);
		if (InitCanvas.Visible) InitCanvas.HandleProgress(report);
	}

	public async void SaveCharacter() {
		if (File == null || EditCanvas.SaveButton.Disabled) return;
		EditCanvas.SaveButton.Disabled = true;
		string? error = await File.Write(progress: HandleProgress);
		if (error != null) EditCanvas.SaveError.Text = error;
		EditCanvas.SaveButton.Disabled = false;
	}

	public async Task LoadCharacter(string path) {
		var result = await CharacterFile.Read(path, progress: HandleProgress);
		if (result.LetErr(out string err)) SetError(err);
		if (!result.LetOk(out var file)) return;
		File = file;
		EmitSignalCharacterLoaded();
		SceneTransition.FadeIn(loading: true, finished: () => {
			InitCanvas.Visible = false;
			EditCanvas.Visible = true;
			LoadModel(file.Model);
			SceneTransition.FadeOut();
		});
	}

	public async Task NewCharacter(string path) {
		SceneTransition.FadeInAsync(loading: true, finished: async () => {
			File = new CharacterFile { FilePath = path };
			string? error = await File.Write(progress: HandleProgress);
			SetError(error);

			InitCanvas.Visible = false;
			EditCanvas.Visible = true;
			LoadModel(File.Model);
			SceneTransition.FadeOut();
		});
	}

	public async Task LoadModel(string path) {
		if (File == null) return;
		var result = await Model.Load(path);
		if (result.IsErr) {
			SetError(result.Error!);
			return;
		}
		LoadModel(result.Value);
	}

	public void LoadModel(Model? model) {
		File?.Model = model;
		if (!model.HasValue) return;
		var document = model.Value.Document;
		var state = model.Value.State;

		var root = document.GenerateScene(state, bakeFps: 60f, trimming: true);
		if (root == null) {
			SetError("Failed to create model scene");
			return;
		}
		foreach (var child in BotHolder.GetChildren()) child?.QueueFree();
		BotHolder.AddChild(root);
	}
}

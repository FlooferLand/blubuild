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
		MusicGlobal.Play(MusicGlobal.Track.CharacterEditor);
	}

	public void SetError(string? error) {
		Callable.From(() => {
			if (InitCanvas.Visible) InitCanvas.SetError(error);
			if (EditCanvas.Visible) EditCanvas.SetError(error);
		}).CallDeferred();
	}

	// TODO: Make the progress add up over time. Currently it changes per item.
	public void HandleProgress(ProgressHolder holder) {
		Callable.From(() => {
			holder.UpdateUi(EditCanvas.SaveProgressBar);
			holder.UpdateUi(EditCanvas.Info, makeInvis:true);
			if (InitCanvas.Visible) {
				holder.UpdateUi(InitCanvas.Progress, makeInvis:true);
				holder.UpdateUi(InitCanvas.Info, makeInvis:true);
			}
		}).CallDeferred();
	}

	public async Task SaveCharacter() {
		if (File == null || EditCanvas.SaveButton.Disabled) return;
		EditCanvas.SaveButton.Disabled = true;
		EditCanvas.SaveProgressBar.Value = 0;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		using var progress = new ProgressHolder("Saving the character", HandleProgress);
		string? error = await File.Write(progress);
		if (error != null) EditCanvas.Error.Text = error;
		EditCanvas.SaveButton.Disabled = false;
	}

	public async Task LoadCharacter(string path) {
		using var progress = new ProgressHolder("Loading the character", HandleProgress);
		var result = await CharacterFile.Read(path, progress);
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
		await SceneTransition.FadeInAsync(loading: true, finished: async () => {
			using var progress = new ProgressHolder("Creating the character", HandleProgress);
			File = new CharacterFile { FilePath = path };
			string? error = await File.Write(progress);
			Callable.From(() => {
				SetError(error);
				InitCanvas.Visible = false;
				EditCanvas.Visible = true;
				LoadModel(File.Model);
				SceneTransition.FadeOut();
			}).CallDeferred();
		});
	}

	public async Task LoadModel(string path) {
		using var progress = new ProgressHolder("Loading the model", HandleProgress);
		if (File == null) return;
		var result = await Model.Load(path, progress);
		if (result.IsErr) {
			SetError(result.Error!);
			return;
		}
		LoadModel(result.Value);
	}

	public void LoadModel(Model? model) {
		using var progress = new ProgressHolder("Creating a scene", HandleProgress);

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

using System.Threading.Tasks;
using Godot;

namespace Project.CharacterEditing;

public partial class CharacterEditor : Node3D {
	[Signal] public delegate void CharacterLoadedEventHandler();
	[Export] public required Node3D BotHolder;
	[Export] public required SpringArm3D CameraHolder;
	[Export] public required Camera3D Camera;
	[Export] public required Transition SceneTransition;
	[Export] public required InitCanvas InitCanvas;
	[Export] public required EditCanvas EditCanvas;

	const float maxDistance = 2.0f;

	public CharacterFile? File = null;
	Vector3 rotationTarget = Vector3.Zero;
	Vector3 positionTarget = Vector3.Zero;
	bool panning = false;
	bool moving = false;
	float zoom = 1.0f;

	public override void _Ready() {
		InitCanvas.Visible = true;
		EditCanvas.Visible = false;
		SceneTransition.FadeOut();
		MusicGlobal.Play(MusicGlobal.Track.CharacterEditor);
		rotationTarget = CameraHolder.Rotation;
		positionTarget = CameraHolder.Position;
	}

	public override void _Input(InputEvent @event) {
		switch (@event) {
			case InputEventMouseButton button:
				panning = button is { Pressed: true, ButtonIndex: MouseButton.Right };
				moving = button is { Pressed: true, ButtonIndex: MouseButton.Middle };
				switch (button) {
					case { Pressed: true, ButtonIndex: MouseButton.WheelUp }:
						zoom -= 0.1f;
						break;
					case { Pressed: true, ButtonIndex: MouseButton.WheelDown }:
						zoom += 0.1f;
						break;
				}
				zoom = Mathf.Clamp(zoom, 0.4f, 1.5f);
				break;
			case InputEventMouseMotion motion:
				if (panning) {
					rotationTarget -= new Vector3(motion.Relative.Y, motion.Relative.X, 0f) * 0.01f;
				} else if (moving) {var basis = Camera.GlobalTransform.Basis;
					var center = Vector3.Up * 1.0f;
					positionTarget += (basis.X * -motion.Relative.X + basis.Y * motion.Relative.Y) * 0.005f * zoom;
					var offset = positionTarget - center;
					positionTarget = center + offset.LimitLength(maxDistance);
					positionTarget.Y = Mathf.Max(positionTarget.Y, 0.1f);
				}
				break;
		}
	}

	public override void _Process(double delta) {
		float weight = 14f * (float) delta;
		CameraHolder.Rotation = CameraHolder.Rotation.Slerp(rotationTarget, weight);
		CameraHolder.Position = CameraHolder.Position.Lerp(positionTarget, weight);
		CameraHolder.SpringLength = Mathf.Lerp(CameraHolder.SpringLength, 2f * zoom, 6f * (float) delta);
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

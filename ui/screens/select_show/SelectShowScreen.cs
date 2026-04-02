using Godot;
using Godot.Collections;

namespace Project;

public partial class SelectShowScreen : AbstractScreen {
	[Export] public required string ShowDir;

	[ExportGroup("Local")]
	[Export] public required Container ButtonContainer;

	Array<string> showFiles = new();

	public override void _EnterTree() {
		base._EnterTree();
		ScreenDeactivate();
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (Input.IsActionJustPressed("debug_load_show")) {
			ScreenToggle();
		}
	}

	protected override void OnScreenOpen() {
		Rpc(nameof(ClientGetFiles));
	}
	protected override void OnScreenRemove() {

	}

	void ClientNewFilesReceived() {
		foreach (var child in ButtonContainer.GetChildren()) child?.Free();
		foreach (string path in showFiles) {
			var button = new Button { Text = $"{path.GetFile()}" };
			button.Pressed += () => {

				ScreenDeactivate();
			};
			ButtonContainer.AddChild(button);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	void ClientGetFiles() {
		showFiles.Clear();
		foreach (string file in DirAccess.Open(ShowDir).GetFiles()) {
			string path = ShowDir.PathJoin(file);
			showFiles.Add(path);
		}
		ClientNewFilesReceived();
	}
}

using System;

namespace Project;
using Godot;

// Only runs client-side; Used to send input to the server
public partial class MultiplayerInput : MultiplayerSynchronizer {
	public Vector2 InputDirection = Vector2.Zero;
	public Vector2 InputMouse = Vector2.Zero;
	public bool FlyCamEnabled = false;

	public override void _Ready() {
		// Stop processing in this class from running on the server
		if (!LocalPeer.ThisClientOwns(this) || !LocalPeer.IsClient) {
			SetProcess(false);
			SetProcessInput(false);
			SetPhysicsProcess(false);
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion motion) {
			InputMouse = (motion.Relative * 0.002f);
		}

		// TODO: Finish networking the interaction system
		// if (Input.IsActionPressed("interact_primary")) {
		// 	GlobalStorage.LocalPlayer?.Rpc(nameof(GlobalStorage.LocalPlayer.ApplyInteractionServerside), true);
		// }
		// else if (Input.IsActionPressed("interact_secondary")) {
		// 	GlobalStorage.LocalPlayer?.Rpc(nameof(GlobalStorage.LocalPlayer.ApplyInteractionServerside), false);
		// }

		if (Input.IsActionPressed("debug")) {
			FlyCamEnabled = !FlyCamEnabled;
			GlobalStorage.LocalPlayer?.Rpc(nameof(GlobalStorage.LocalPlayer.SetFlyCam), FlyCamEnabled);
		} else if (Input.IsActionJustPressed("window_toggle_focus") && GetWindow().HasFocus()) {
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible
				? Input.MouseModeEnum.Captured
				: Input.MouseModeEnum.Visible;
		} else if (Input.IsActionJustPressed("pause")) {
			GetTree().Quit();
		}
	}

	public override void _Process(double delta) {
		InputDirection = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		InputMouse = Vector2.Zero;
	}
}

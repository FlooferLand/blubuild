namespace Project;
using Godot;

// Only runs client-side; Used to send input to the server
public partial class PlayerInput : MultiplayerSynchronizer {
	public Vector2 InputDirection = Vector2.Zero;
	public Vector2 InputMouse = Vector2.Zero;

	public override void _Ready() {
		// Stop processing in this class from running on the server
		if (!GetTree().ClientOwns(this) || !Multiplayer.IsClientOrIntegrated()) {
			SetProcess(false);
			SetProcessInput(false);
			SetPhysicsProcess(false);
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion motion) {
			InputMouse = (motion.Relative * 0.002f);
		}

		// Interaction
		if (Input.IsActionJustPressed("interact_primary")) {
			TriggerInteraction(InteractType.Primary, InteractState.Press);
		} else if (Input.IsActionJustReleased("interact_primary")) {
			TriggerInteraction(InteractType.Primary, InteractState.Release);
		}
		if (Input.IsActionJustPressed("interact_secondary")) {
			TriggerInteraction(InteractType.Secondary, InteractState.Press);
		} else if (Input.IsActionJustReleased("interact_secondary")) {
			TriggerInteraction(InteractType.Secondary, InteractState.Release);
		}

		// Misc stuff
		if (Input.IsActionJustPressed("window_toggle_focus") && GetWindow().HasFocus()) {
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible
				? Input.MouseModeEnum.Captured
				: Input.MouseModeEnum.Visible;
		}
	}

	public override void _Process(double delta) {
		InputDirection = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		InputMouse = Vector2.Zero;
	}

	void TriggerInteraction(InteractType type, InteractState state) {
		BlubuildClient.LocalPlayer?.RpcToServer(nameof(Player.Server_ApplyInteraction), (int) type, (int) state);
	}
}

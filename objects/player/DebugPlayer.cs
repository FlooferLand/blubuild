namespace Project;
using Godot;

public partial class DebugPlayer : Node3D {
	// Settings
	private const float Speed = 10.0f;
	public float MouseSensitivity = 1.0f;
	[Export] public int PlayerId = 1;
	
	// Nodes
	[Export] public Camera3D Camera;
	public MultiplayerInput InputSynchronizer;
	
	public void SetNetworkPlayerId(int id) {
		PlayerId = id;
		InputSynchronizer!.SetMultiplayerAuthority(id);
	}
	
	public override void _PhysicsProcess(double delta) {
		// Camera
		var mouseInput = InputSynchronizer.InputMouse * MouseSensitivity;
		RotateY(-mouseInput.X);
		RotateX(-mouseInput.Y);
		var rot = Rotation;
		rot.X = Mathf.Clamp(Rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);
		rot.Z = 0f;
		Rotation = rot;
		
		// Movement
		var inputDir = InputSynchronizer.InputDirection;
		var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y).Normalized());
		GlobalPosition += (direction * Speed * (float) delta);
	}
}

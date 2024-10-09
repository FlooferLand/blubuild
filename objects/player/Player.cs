using Godot;
using GodotUtils;

namespace Project;

public partial class Player : CharacterBody3D {
	// Settings
	private const float Speed = 4.0f;
	private const float Acceleration = 0.6f;
	private const float JumpHeight = 4.0f;
	public float MouseSensitivity = 1.0f;
	
	// Nodes
	[ExportGroup("Nodes")]
	[Export] public Node3D Head;
	[Export] public Camera3D Camera;
	[Export] public RayCast3D InteractRay;
	[Export] public Node3D BodyPivot;
	[Export] public MultiplayerInput InputSynchronizer;
	
	// Resources
	[ExportGroup("Resources")]
	[Export] public PackedScene DebugPlayerScene;
	
	// Variables
	[ExportGroup("Variables")]
	private float gravity = (float) ProjectSettings.GetSetting("physics/3d/default_gravity");
	private bool jumping = false;
	private float speed = Speed;
	[Export] public int PlayerId = -1;
	
	public void SetNetworkPlayerId(int id) {
		PlayerId = id;
		GlobalStorage.LocalPlayer = this;
		InputSynchronizer.SetMultiplayerAuthority(id);
	}
	
	public override void _Ready() {
		if (LocalPeer.ThisClientOwns(this)) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
			Camera.MakeCurrent();
		} else {
			Camera.ClearCurrent();
		}
	}

	public override void _PhysicsProcess(double delta) {
		ApplyMovementServerside(delta);
		if (Rotation != Vector3.Zero) {
			GD.PushWarning($"Player rotation should be achieved with {nameof(BodyPivot)}! Rotation was set on {Name}");
			Rotation = Vector3.Zero;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ApplyInteractionServerside() {
		Log.Debug(InteractRay.GetCollider()?.ToString());
	}

	public void ApplyMovementServerside(double delta) {
		if (InputSynchronizer.FlyCamEnabled) return;
		
		// Camera
		var mouseInput = InputSynchronizer.InputMouse * MouseSensitivity;
		BodyPivot.RotateY(-mouseInput.X);
		Head.RotateX(-mouseInput.Y);
		var rot = Head.Rotation;
		rot.X = Mathf.Clamp(Head.Rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);
		Head.Rotation = rot;
		
		// Gravity
		if (!IsOnFloor())
			Velocity = Velocity.WithY(Velocity.Y - (gravity * delta));
		
		// Movement
		var inputDir = InputSynchronizer.InputDirection;
		var direction = (BodyPivot.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y).Normalized());
		if (!direction.IsZeroApprox()) {
			Velocity = Velocity.WithX(direction.X * Speed);
			Velocity = Velocity.WithZ(direction.Z * Speed);
		} else {
			Velocity = Velocity.WithX(Mathf.MoveToward(direction.X * Speed, 0f, Acceleration * delta));
			Velocity = Velocity.WithZ(Mathf.MoveToward(direction.Z * Speed, 0f, Acceleration * delta));
		}
		MoveAndSlide();
		
		// Footstep sounds
		if (IsOnFloor()) {
			// TODO: Footstep sounds
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetFlyCam(bool active) {
		string name = Name + "-DEBUG";
		
		// Removing an already existing one
		GetParent().GetNodeOrNull<DebugPlayer>(name)?.QueueFree();
		if (!active || !OS.IsDebugBuild())
			return;

		bool isOwner = LocalPeer.ThisClientOwns(this);
		
		// Adding a new debug player
		var debugPlayer = DebugPlayerScene.Instantiate<DebugPlayer>();
		debugPlayer.InputSynchronizer = InputSynchronizer;
		debugPlayer.Camera.Current = isOwner;
		debugPlayer.SetNetworkPlayerId(PlayerId);
		Camera.Current = !isOwner;
		
		GetParent().AddChild(debugPlayer);
	}
}

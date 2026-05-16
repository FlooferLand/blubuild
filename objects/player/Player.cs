using System;
using System.Diagnostics;
using Godot;
using GodotUtils;

namespace Project;

public partial class Player : CharacterBody3D {
	// Settings
	const float Speed = 4.0f;
	const float Acceleration = 0.6f;
	const float JumpHeight = 4.0f;
	public float MouseSensitivity = 1.0f;
	
	// Nodes
	[ExportGroup("Local")]
	[Export] public required Node3D Head;
	[Export] public required Camera3D Camera;
	[Export] public required RayCast3D InteractRay;
	[Export] public required Node3D BodyPivot;
	[Export] public required PlayerInput PlayerInput;
	[Export] public required Label3D Nameplate;
	[Export] public required MeshInstance3D Model;
	[Export] public required Control Crosshair;

	// Variables
	[ExportGroup("Variables")]
	float gravity = (float) ProjectSettings.GetSetting("physics/3d/default_gravity");
	bool jumping = false;
	float speed = Speed;
	[Export] public int PlayerId = -1;
	[Export] public string PlayerUsername {
		get {
			if (!IsInsideTree()) return "";
			return Nameplate.Text ?? "";
		}
		set {
			if (!IsInsideTree()) return;
			Nameplate.Text = value;
		}
	}
	IPlayerInteractable? hovered = null;

	public void SetNetworkPlayerId(int id) {
		PlayerId = id;
		if (Multiplayer.GetUniqueId() == id) BlubuildClient.LocalPlayer = this;
		PlayerInput.SetMultiplayerAuthority(id);
	}
	
	public override void _Ready() {
		if (this == BlubuildClient.LocalPlayer) {
			if (!Debugger.IsAttached) Input.MouseMode = Input.MouseModeEnum.Captured;
			Camera.MakeCurrent();
			Nameplate.Visible = false;
			Model.Visible = false;
		} else {
			Camera.ClearCurrent();
			Nameplate.Visible = true;
			Model.Visible = true;
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (Multiplayer.IsServer()) ApplyMovementServerside(delta);
		if (Rotation != Vector3.Zero) {
			GD.PushWarning($"Player rotation should be achieved with {nameof(BodyPivot)}! Rotation was set on {Name}");
			Rotation = Vector3.Zero;
		}

		// Interaction
		hovered = null;
		InteractRay.ForceRaycastUpdate();
		if (InteractRay.GetCollider() is Node and IPlayerInteractable interactable)
			hovered = interactable;
		Crosshair.Modulate = hovered != null ? Colors.White : Colors.Transparent;
	}

	void ApplyMovementServerside(double delta) {
		// Camera
		var mouseInput = PlayerInput.InputMouse * MouseSensitivity;
		BodyPivot.RotateY(-mouseInput.X);
		Head.RotateX(-mouseInput.Y);
		var rot = Head.Rotation;
		rot.X = Mathf.Clamp(Head.Rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);
		Head.Rotation = rot;
		
		// Gravity
		if (!IsOnFloor())
			Velocity = Velocity.WithY(Velocity.Y - (gravity * delta));
		
		// Movement
		var inputDir = PlayerInput.InputDirection;
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = TransferChannels.PlayerMessages, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public async void Server_ApplyInteraction(InteractType type, InteractState state) {
		try {
			if (!Multiplayer.IsServer()) return;
			if (hovered is not {} collider) return;

			await collider.Interact(new InteractContext(this, type, state));
			var path = ((Node) collider).GetPath();
			Rpc(nameof(Client_ApplyInteraction), path, (int) type, (int) state);
		} catch (Exception e) {
			Log.Error(e);
		}
	}

	[Rpc(CallLocal = false, TransferChannel = TransferChannels.PlayerMessages, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	async void Client_ApplyInteraction(NodePath interactor, InteractType type, InteractState state) {
		try {
			var node = (IPlayerInteractable) GetNode(interactor);
			await node.Interact(new InteractContext(this, type, state));
		} catch (Exception e) {
			Log.Error(e);
		}
	}
}

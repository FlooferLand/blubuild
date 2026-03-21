using System.Collections.Generic;
using Godot;
using GodotUtils;

namespace Project;

public partial class NetworkManager : Node {
	// Constants
	const int ServerPort = 1712;
	const string ServerAddress = "127.0.0.1";

	// Nodes
	[Export] public required PackedScene PlayerScene;
	[Export] public required Node3D PlayerContainer;

	public static bool IsIntegratedServer = false;
	public static Dictionary<long, Player> Players = new();

	public override void _EnterTree() {
		var window = GetWindow();
		var clientNum = SystemAutoload.Args.GetValueFlag<int>("client");
		bool isServer = SystemAutoload.Args.GetBoolFlag("server");
		
		// If the server, or a client
		window.Size = new Vector2I(900, 648);
		if (isServer) {
			SystemAutoload.TitleNetworkingType = "server";
			window.Mode = Window.ModeEnum.Minimized;
			window.MinSize = Vector2I.One * 10;
			window.MaxSize = Vector2I.One * 10;
			window.Size = Vector2I.One * 10;
			StartServer();
			return;
		}
		if (clientNum.LetSome(out int num)) {
			window.Position = (DisplayServer.ScreenGetUsableRect(num % 2).GetCenter() - (window.Size / 2)) + (Vector2I.One * 50);
			SystemAutoload.TitleNetworkingType = "client";
			SystemAutoload.ClientNum = num;
			window.GrabFocus();
			StartClient(num);
			return;
		}

		// Integrated server with client (no arguments)
		window.MoveToCenter();
		SystemAutoload.TitleNetworkingType = "singleplayer";
		IsIntegratedServer = true;
		StartServer();
	}

	public override void _ExitTree() {
		GD.Print("Network manager destroyed");
		if (Multiplayer.IsServer()) {
			Multiplayer.Free();
		}
	}

	#region Management
	public void StartServer() {
		Log.Info("Starting server" + (IsIntegratedServer ? " (integrated)" : ""));

		var peer = new ENetMultiplayerPeer();
		peer.CreateServer(ServerPort);

		var multiplayer = ((SceneMultiplayer) Multiplayer);
		multiplayer.MultiplayerPeer = peer;
		multiplayer.PeerPacket += NetworkTransport.OnPeerPacket;
		multiplayer.PeerConnected += AddPlayer;
		multiplayer.PeerDisconnected += p => {
			RemovePlayer(p);
			if (OS.IsDebugBuild() && multiplayer.GetPeers().Length == 0) {
				GetTree().Quit();
			}
		};

		if (IsIntegratedServer) {
			AddPlayer(peer.GetUniqueId());
		} else {
			// For development
			var node = new Node();
			node.Name = MultiplayerPeer.TargetPeerServer.ToString();
			PlayerContainer.AddChild(node);
		}
	}

	public void StartClient(int num) {
		Log.Info($"Starting client {num}");

		var peer = new ENetMultiplayerPeer();
		peer.CreateClient(ServerAddress, ServerPort);

		var multiplayer = ((SceneMultiplayer) Multiplayer);
		multiplayer.MultiplayerPeer = peer;
		multiplayer.PeerPacket += NetworkTransport.OnPeerPacket;
		multiplayer.PeerConnected += AddPlayer;
		multiplayer.PeerDisconnected += RemovePlayer;
		multiplayer.ConnectedToServer += () => AddPlayer(Multiplayer.GetUniqueId());
	}

	public void AddPlayer(long id) {
		if (id == 1 && !IsIntegratedServer)  // Don't add the server as a player
			return;
		
		var player = PlayerScene.Instantiate<Player>();
		player.SetNetworkPlayerId((int) id);
		player.Name = id.ToString();
		player.TreeEntered += () => {
			player.GlobalPosition = new Vector3(Players.Count * 2.0f, 0f, 0f);
		};

		PlayerContainer.AddChild(player, forceReadableName:true);
		Players.Add(id, player);
	}

	public void RemovePlayer(long id) {
		if (PlayerContainer.GetNode(id.ToString()) is {} node) {
			node.QueueFree();
		}
	}
	#endregion
}

using Godot;
using GodotUtils;
using System.Linq;

namespace Project;

public partial class NetworkManager : Node {
	// Constants
	private const int ServerPort = 1712;
	private const string ServerAddress = "127.0.0.1";

	// Nodes
	[Export] public Node3D PlayerSpawnNode;
	private PackedScene playerScene = GD.Load<PackedScene>("res://objects/player/player.tscn");
	
	// State

	public override void _EnterTree() {
		var window = GetWindow();
		var clientNum = SystemAutoload.Args.GetValueFlag<int>("client");
		bool isServer = SystemAutoload.Args.GetBoolFlag("server");

		// If the server, or a client
		window.Size = new Vector2I(900, 648);
		if (isServer) {
			SystemAutoload.TitleNetworkingType = "server";
			window.Mode = Window.ModeEnum.Minimized;
			LocalPeer.Init(true, false, false);
			StartServer();
		} else {
			if (clientNum.HasValue) {
				var pos = (DisplayServer.ScreenGetSize() / 2) - (window.Size / 2);
				pos.X += (window.Size.X - (window.Size.X / 2)) * (clientNum.UnwrapOr(0) == 1 ? -1 : 1);
				window.Position = pos;
				SystemAutoload.TitleNetworkingType = "client";
				LocalPeer.Init(false, true, false);
				StartClient();
			} else {  // Integrated server with client (no arguments)
				window.MoveToCenter();
				SystemAutoload.TitleNetworkingType = "singleplayer";
				LocalPeer.Init(true, true, true);
				StartServer();
			}
		}
	}

	public override void _ExitTree() {
		if (Multiplayer.IsServer()) {
			Multiplayer.Free();
		}
	}

	#region Management
	public void StartServer() {
		Log.Info($"Starting server ({nameof(LocalPeer.IntegratedServer)}={LocalPeer.IntegratedServer})");

		var peer = new ENetMultiplayerPeer();
		peer.CreateServer(ServerPort);
		LocalPeer.PeerId = peer.GetUniqueId();

		Multiplayer.MultiplayerPeer = peer;
		Multiplayer.PeerConnected += AddPlayer;
		Multiplayer.PeerDisconnected += p => {
			RemPlayer(p);
			if (OS.IsDebugBuild() && Multiplayer.GetPeers().Length == 0) {
				GetTree().Quit();
			}
		};
		if (LocalPeer.IntegratedServer) AddPlayer(peer.GetUniqueId());
	}

	public void StartClient() {
		Log.Info("Starting client");

		var peer = new ENetMultiplayerPeer();
		peer.CreateClient(ServerAddress, ServerPort);

		Multiplayer.MultiplayerPeer = peer;
		Multiplayer.PeerConnected += AddPlayer;
		Multiplayer.PeerDisconnected += RemPlayer;
		AddPlayer(peer.GetUniqueId());
	}

	public void AddPlayer(long id) {
		if (id == 1 && !LocalPeer.IntegratedServer)  // Don't add the server as a player
			return;
		
		var player = playerScene.Instantiate<Player>();
		player.SetNetworkPlayerId((int) id);
		player.Name = id.ToString();
		LocalPeer.PeerId = id;
		
		PlayerSpawnNode.AddChild(player, true);
	}

	public void RemPlayer(long id) {
		if (PlayerSpawnNode.GetNode(id.ToString()) is {} node) {
			node.QueueFree();
		}
	}
	#endregion
}

using System.Collections.Generic;
using Godot;
using GodotUtils;

namespace Project;

public partial class NetworkManager : Node {
	// Constants
	public const int ServerPort = 1712;
	public const string ServerAddress = "127.0.0.1";

	// Temporary stores for client host/join stuff
	public static string? Username = null;
	public static string? JoinAddress = null;

	// Nodes
	[Export] public required PackedScene PlayerScene;
	[Export] public required Node3D PlayerContainer;

	public static bool IsIntegratedServer = false;
	public static bool IsDedicatedServer = false;
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
			window.SetCurrentScreen(1);
			IsDedicatedServer = true;
			StartServer();
			return;
		}
		if (clientNum.LetSome(out int num)) {
			window.Position = (DisplayServer.ScreenGetUsableRect(num % 2).GetCenter() - (window.Size / 2)) + (Vector2I.One * 50);
			SystemAutoload.TitleNetworkingType = "client";
			SystemAutoload.ClientNum = num;
			window.GrabFocus();
			StartClient();
			return;
		}

		// Joining a remote server
		if (JoinAddress != null) {
			GD.Print($"Joining remote address '{JoinAddress}'");
			SystemAutoload.TitleNetworkingType = "client (connected to remote)";
			SystemAutoload.ClientNum = 1;
			StartClient();
		}

		// Integrated server with client (no arguments)
		if (JoinAddress == null) {
			window.MoveToCenter();
			SystemAutoload.TitleNetworkingType = "singleplayer";
			IsIntegratedServer = true;
			StartServer();
		}
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
		var status = peer.CreateServer(ServerPort);
		if (status != Error.Ok) {
			OS.Alert($"Status: {status}", "Failed to create server");
			GetTree().Quit(-1);
		}

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
			// Dedicated server
			for (int i = 0; i < AudioServer.GetBusCount(); i++) {
				AudioServer.SetBusVolumeLinear(i, 0f);
			}

			// For development
			var node = new Node();
			node.Name = MultiplayerPeer.TargetPeerServer.ToString();
			PlayerContainer.AddChild(node);
		}
	}

	public void StartClient() {
		Log.Info($"Starting client {SystemAutoload.ClientNum}");

		var peer = new ENetMultiplayerPeer();
		var status = peer.CreateClient(JoinAddress ?? ServerAddress, ServerPort);
		if (status != Error.Ok) {
			OS.Alert($"Status: {status}", "Failed to join server");
			GetTree().Quit(-1);
		}

		var multiplayer = ((SceneMultiplayer) Multiplayer);
		multiplayer.MultiplayerPeer = peer;
		multiplayer.PeerPacket += NetworkTransport.OnPeerPacket;
		multiplayer.PeerConnected += AddPlayer;
		multiplayer.PeerDisconnected += RemovePlayer;
		multiplayer.ServerDisconnected += () => GetTree().Quit();
		multiplayer.ConnectedToServer += () => AddPlayer(Multiplayer.GetUniqueId());
		multiplayer.ConnectionFailed += () => OS.Alert("No other details", "Connection failed");
	}

	public void AddPlayer(long id) {
		if (id == 1 && IsDedicatedServer)  // Don't add the dedicated server as a player
			return;

		var player = PlayerScene.Instantiate<Player>();
		player.Name = id.ToString();
		player.TreeEntered += () => {
			player.SetNetworkPlayerId((int) id);
			player.GlobalPosition = new Vector3(Players.Count * 2.0f, 0f, 0f);

			// Client telling the server its username
			if (BlubuildClient.LocalPlayer == player) {
				RpcId(1, nameof(AnnouncePlayerUsername), id, Username!);
			}
			if (Multiplayer.IsServer() && id != 1) {
				foreach (var entry in Players) {
					RpcId(id, nameof(AnnouncePlayerUsername), entry.Key, entry.Value.PlayerUsername);
				}
			}
		};
		Players.Add(id, player);
		PlayerContainer.AddChild(player, forceReadableName:true);
	}

	public void RemovePlayer(long id) {
		if (PlayerContainer.GetNodeOrNull(id.ToString()) is {} node) node.QueueFree();
		Players.Remove(id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = TransferChannels.PlayerMessages)]
	public void AnnouncePlayerUsername(long playerId, string username) {
		if (!Players.TryGetValue(playerId, out var player)) {
			Log.Warning($"No player found '{playerId}'");
			return;
		}
		player.PlayerUsername = username;
		this.ServerRpcToClients(nameof(AnnouncePlayerUsername), playerId, username);
	}
	#endregion
}

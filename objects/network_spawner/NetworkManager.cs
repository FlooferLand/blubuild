using System.Linq;

namespace Project;
using Godot;

public partial class NetworkManager : Node {
	private const int ServerPort = 1712;
	private const string ServerAddress = "127.0.0.1";

	private PackedScene playerScene = GD.Load<PackedScene>("res://objects/player/player.tscn");
	[Export] public Node3D PlayerSpawnNode;

	public override void _EnterTree() {
		string[] args = OS.GetCmdlineArgs();
		var window = GetWindow();

		int debugClientNum = 0;
		if (args.Contains("--client1"))
			debugClientNum = -1;
		else if (args.Contains("--client2"))
			debugClientNum = 1;

		// If the server, or a client
		window.Size = new Vector2I(900, 648);
		if (args.Contains("--server")) {
			window.Title = "SERVER";
			window.Mode = Window.ModeEnum.Minimized;
			StartServer();
		} else {
			if (debugClientNum != 0) {
				var pos = (DisplayServer.ScreenGetSize() / 2) - (window.Size / 2);
				pos.X += (window.Size.X - (window.Size.X / 2)) * debugClientNum;
				window.Position = pos;
				OS.DelayMsec(200);
			}
			window.Title = "CLIENT";
			StartClient();
		}
	}

	public override void _ExitTree() {
		if (Multiplayer.IsServer()) {
			Multiplayer.Free();
		}
	}

	public void StartServer() {
		Log.Info("Starting server");

		var serverPeer = new ENetMultiplayerPeer();
		serverPeer.CreateServer(ServerPort);

		Multiplayer.MultiplayerPeer = serverPeer;
		Multiplayer.PeerConnected += AddPlayer;
		Multiplayer.PeerDisconnected += (p) => {
			RemPlayer(p);
			if (OS.IsDebugBuild() && Multiplayer.GetPeers().Length == 0) {
				GetTree().Quit();
			}
		};
	}

	public void StartClient() {
		Log.Info("Starting client");

		var clientPeer = new ENetMultiplayerPeer();
		clientPeer.CreateClient(ServerAddress, ServerPort);

		Multiplayer.MultiplayerPeer = clientPeer;
		Multiplayer.PeerConnected += AddPlayer;
		Multiplayer.PeerDisconnected += RemPlayer;
		AddPlayer(clientPeer.GetUniqueId());
	}

	public void AddPlayer(long id) {
		if (id == 1)  // Don't add the server as a player
			return;
		
		var player = playerScene.Instantiate<Player>();
		player.SetNetworkPlayerId((int) id);
		player.Name = id.ToString();
		
		PlayerSpawnNode.AddChild(player, true);
	}

	public void RemPlayer(long id) {
		if (PlayerSpawnNode.GetNode(id.ToString()) is {} node) {
			node.QueueFree();
		}
	}
}

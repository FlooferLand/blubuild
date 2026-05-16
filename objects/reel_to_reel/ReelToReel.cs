using System.Threading.Tasks;
using Bluchalk;
using Bluchalk.shows;
using Godot;
using GodotUtils;

namespace Project;

/// Handles show file loading and reading, just like the real one!
public partial class ReelToReel : Node3D, IPlayerInteractable {
	[Signal] public delegate void ShowLoadedEventHandler();

	[Export] public required string ShowDir;
	[Export] public required string ShowFile;
	[Export] public required Greybox Greybox;

	[ExportGroup("Variables")]
	[Export] public double Seek = 0;  // Synced via node

	[ExportGroup("Local")]
	[Export] public required MultiplayerSynchronizer SeekSync;

	public readonly RshwFormat format = new();
	public RshwFormat.RshwData? ServerLoadedShow = null;
	public WavHeader? ServerLoadedWav = null;
	public string? ServerLoadedChart = null;

	// RR signal data is locked to 60
	public const double RshwTargetFps = 60;
	public const double FrameDuration = 1.0 / RshwTargetFps;

	public override void _PhysicsProcess(double delta) {
		if (ServerLoadedShow is not null) {
			Seek += delta;
		}
	}

	public async ValueTask Interact(InteractContext ctx) {
		if (ctx.State == InteractState.Press) {
			await Play();
		}
	}

	/// Plays the default song if no path is provided
	public async Task Play(string? path = null) {
		if (!Multiplayer.IsServer()) return;

		path ??= ShowDir.PathJoin(ShowFile);

		var result = await Task.Run(() => format.ReadFile(path));
		if (result.LetErr(out string err)) {
			GD.PushError($"Failed to load show: {err}");
			return;
		}
		if (!result.LetOk(out var data)) return;

		Seek = 0;
		ServerLoadedShow = data;
		ServerLoadedWav = WavHeader.Read(data.audio);
		ServerLoadedChart = null;
		if (BitChartRegistry.ExtensionToId.TryGetValue(path.GetExtension().ToLower(), out string? id)) {
			ServerLoadedChart = id;
		}

		Log.Info($"Loading show '{path.GetBaseName()}' (Channels={ServerLoadedWav.Value.Channels}, Bits={ServerLoadedWav.Value.Bits})");
		Rpc(nameof(AnnounceShowLoaded));
	}

	[Rpc(CallLocal = true, TransferChannel = TransferChannels.ShowMessage, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void AnnounceShowLoaded() {
		EmitSignalShowLoaded();
	}
}

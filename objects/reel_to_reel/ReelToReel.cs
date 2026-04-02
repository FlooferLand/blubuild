using Bluchalk;
using Bluchalk.shows;
using Godot;
using GodotUtils;

namespace Project;

/// Handles show file loading and reading, just like the real one!
public partial class ReelToReel : Node3D {
	[Signal] public delegate void ServerShowLoadedEventHandler();

	[Export] public required string ShowDir;
	[Export] public required string ShowFile;
	[Export] public required Greybox Greybox;

	[ExportGroup("Variables")]
	[Export] public double Seek = 0;  // Synced via node

	[ExportGroup("Local")]
	[Export] public required MultiplayerSynchronizer SeekSync;
	[Export] public required Timer TempLoadTime;

	public readonly RshwFormat format = new();
	public RshwFormat.RshwData? ServerLoadedShow = null;
	public WavHeader? ServerLoadedWav = null;

	// RR signal data is locked to 60
	public const double RshwTargetFps = 60;
	public const double FrameDuration = 1.0 / RshwTargetFps;

	public override void _EnterTree() {
		TempLoadTime.Timeout += () => Play();
	}

	/// Plays the default song if no path is provided
	public void Play(string? path = null) {
		if (!Multiplayer.IsServer()) {
			return;
		}

		if (path == null)
			path = ShowDir.PathJoin(ShowFile);

		var result = format.ReadFile(path);
		if (result.LetErr(out string err)) {
			GD.PushError($"Failed to load show: {err}");
			return;
		}
		if (!result.LetOk(out var data)) return;
		ServerLoadedShow = data;
		ServerLoadedWav = WavHeader.Read(data.audio);
		Log.Info($"Loading show '{path.GetBaseName()}' (Channels={ServerLoadedWav.Value.Channels}, Bits={ServerLoadedWav.Value.Bits})");
		EmitSignalServerShowLoaded();
	}

	public override void _PhysicsProcess(double delta) {
		Seek += delta;
	}
}

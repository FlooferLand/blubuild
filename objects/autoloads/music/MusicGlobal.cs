using Godot;

namespace Project;

public partial class MusicGlobal : Node {
	[Export] AudioStreamPlayer SongPlayer = null!;

	AudioStreamInteractive? stream = null;
	AudioStreamPlaybackInteractive? playback = null;

	public static AudioStream? CurrentStream {
		get {
			if (Instance?.playback == null || Instance.stream == null) return null;
			int index = Instance.playback.GetCurrentClipIndex();
			return Instance.stream.GetClipStream(index);
		}
	}

	// Mirrors the clip order in the autoload's scene
	public enum Track {
		Empty,
		TitleTheme,
		Runtime,
		CharacterEditor,
		Editor
	}

	public static MusicGlobal? Instance = null;
	public MusicGlobal() {
		Instance = this;
	}
	public override void _Ready() {
		stream = SongPlayer.Stream as AudioStreamInteractive;
		playback = SongPlayer.GetStreamPlayback() as AudioStreamPlaybackInteractive;
	}

	public static void Stop() => Play(Track.Empty);
	public static void Play(Track track) {
		Instance?.playback?.SwitchToClip((int) track);
	}
}

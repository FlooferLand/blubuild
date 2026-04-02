using System.Collections.Generic;
using Godot;

namespace Project;

public partial class BetterAudioPlayer3D : Node3D {
	[Export] public AudioStream? Stream {
		get => _stream;
		set {
			_stream = value;
			AudioPlayer.Stream = value;
		}
	} AudioStream? _stream;

	[Export] public StringName Bus {
		get => _bus;
		set {
			_bus = value;
			AudioPlayer.Bus = value;
		}
	} StringName _bus = "Master";

	[Export] public AudioServer.PlaybackType PlaybackType = AudioServer.PlaybackType.Default;

	[ExportGroup("Local")]
	[Export] public required AudioStreamPlayer3D AudioPlayer;
	[Export] public required RayCast3D ListenRay;

	public StringName AudioBusName = null!;
	AudioEffectLowPassFilter lowPassEffect = null!;

	public Dictionary<int, AudioEffect> extraEffects = new();

	public override void _EnterTree() {
		if (AudioServer.GetBusIndex(Bus) == -1) GD.PushError($"{nameof(BetterAudioPlayer3D)}: Bus '{Bus}' does not exist");
		AudioPlayer.PlaybackType = PlaybackType;

		// Setting up audio bus
		AudioBusName = $"AudioPlayer_{GetInstanceId()}";
		int audioBusIndex = AudioServer.BusCount;
		AudioServer.AddBus();
		AudioServer.SetBusName(audioBusIndex, AudioBusName);
		AudioServer.SetBusSend(audioBusIndex, "Game");
		AudioPlayer.Bus = AudioBusName;

		// Setting up effects
		lowPassEffect = new AudioEffectLowPassFilter();
		lowPassEffect.CutoffHz = 20500f;
		AudioServer.AddBusEffect(audioBusIndex, lowPassEffect);
	}

	public override void _ExitTree() {
		if (AudioServer.GetBusIndex(AudioBusName) is var index && index != -1) {
			AudioServer.RemoveBus(index);
		}
	}

	// TODO: Use proper path tracing to determine how muffled it is
	public override void _Process(double delta) {
		if (BlubuildClient.LocalPlayer is { } player) {
			var globalTarget = player.Head.GlobalPosition;
			ListenRay.TargetPosition = ToLocal(globalTarget);
			bool reachingPlayer = ListenRay.GetCollider() == player;
			float thickness = ListenRay.GetCollisionPoint().DistanceTo(globalTarget) * 1000f;
			lowPassEffect.CutoffHz = Mathf.Lerp(lowPassEffect.CutoffHz, (reachingPlayer ? 20500f : 5000f - thickness), 10f * (float) delta);
		}
	}

	#region AudioStreamPlayer3D compatibility methods
	public void Play(float fromPosition = 0f) {
		AudioPlayer.Play(fromPosition);
	}

	public T? GetStreamPlayback<T>() where T: AudioStreamPlayback {
		return AudioPlayer.GetStreamPlayback() as T;
	}
	#endregion

	#region Bus and effect management
	public int? GetBusId() {
		if (AudioServer.GetBusIndex(AudioBusName) is var id && id != -1) return id;
		return null;
	}

	public AudioEffectHandle<T> AddEffect<T>(T effect) where T : AudioEffect {
		int busId = GetBusId()!.Value;
		int index = AudioServer.GetBusEffectCount(busId);
		AudioServer.AddBusEffect(busId, effect, index);
		extraEffects.Add(index, effect);
		return new AudioEffectHandle<T>(effect, index);
	}

	public void RemoveEffect<T>(AudioEffectHandle<T> handle) where T: AudioEffect {
		int busId = GetBusId()!.Value;
		AudioServer.RemoveBusEffect(busId, handle.Index);
		extraEffects.Remove(handle.Index);
	}

	public I? GetEffectInstance<T, I>(AudioEffectHandle<T> handle) where T : AudioEffect where I : AudioEffectInstance {
		int busId = GetBusId()!.Value;
		return AudioServer.GetBusEffectInstance(busId, handle.Index) as I;
	}

	public struct AudioEffectHandle<T>(T effect, int index) where T : AudioEffect {
		public readonly T Effect = effect;
		public readonly int Index = index;
	}
	#endregion
}

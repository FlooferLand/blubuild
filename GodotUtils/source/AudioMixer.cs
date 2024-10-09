namespace GodotUtils;
using Godot;

/// Bus/effect focused wrapper around AudioServer
/// <b>Heavily recommended for safety to only use this class at startup and cache the results</b>
public class AudioMixerBus {
	private readonly int busId;
	public AudioMixerBus(int id) {
		busId = id;
	}

	/// Gets an effect and casts it to a requested type. Returns <c>null</c> if it can't find the effect.
	/// <b>Heavily recommended to call this at game startup to avoid errors</b>
	public T GetEffect<T>(int effectId) where T: AudioEffect {
		if (AudioServer.GetBusEffect(busId, effectId) is T effect)
			return effect;

		GD.PushWarning($"Couldn't find effect {effectId} on bus {busId}. Perhaps the effect order changed?");
		// Trying to save the situation
		for (int i = 0; i < AudioServer.GetBusEffectCount(busId); i++) {
			if (AudioServer.GetBusEffect(busId, i) is T eff)
				return eff;
		}
		GD.PushError($"Effect {effectId} wasn't found on bus {busId}");
		return null;
	} 
}

public static class AudioMixer {
	public static AudioMixerBus GetBus(string name) {
		int index = AudioServer.GetBusIndex(name);
		return new AudioMixerBus(index);
	}
}

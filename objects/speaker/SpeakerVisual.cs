using Godot;
using GodotUtils;

namespace Project;

public partial class SpeakerVisual : MeshInstance3D {
	[Export] public required BetterAudioPlayer3D AudioPlayer;

	BetterAudioPlayer3D.AudioEffectHandle<AudioEffectSpectrumAnalyzer> analyzerEffect;
	AudioEffectSpectrumAnalyzerInstance analyzerInstance = null!;

	public override void _Ready() {
		analyzerEffect = AudioPlayer.AddEffect(new AudioEffectSpectrumAnalyzer());
		analyzerInstance = AudioPlayer.GetEffectInstance<AudioEffectSpectrumAnalyzer, AudioEffectSpectrumAnalyzerInstance>(analyzerEffect)!;
	}

	public override void _Process(double delta) {
		Scale = Scale.WithY(1.0 + (analyzerInstance.GetMagnitudeForFrequencyRange(0f, 4000f).Combined()));
	}
}

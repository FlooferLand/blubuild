using Godot;
using GodotUtils;

namespace Project;

// TODO: Automatically recalculate the BPM when the stream changes

public partial class BeatTimer : Node {
	[Signal] public delegate void StepEventHandler(int step);
	[Signal] public delegate void BeatEventHandler(int beat);

	double bpm = -1;
	double timeBegin = 0f;
	int step = -1;
	int lastStep = -1;

	public override void _Ready() {
		UpdateBpm(MusicGlobal.CurrentStream);
		MusicGlobal.Instance?.TrackChanged += UpdateBpm;
	}

	void UpdateBpm(AudioStream? stream) {
		timeBegin = Time.GetTicksUsec();
		bpm = stream?.RealBpm ?? 0;
		if (bpm < 1) GD.PushWarning($"{nameof(BeatTimer)} bpm < 1 ({bpm})");
	}

	public override void _Process(double delta) {
		double time = (Time.GetTicksUsec() - timeBegin) / 1_000_000.0;
		time -= AudioServer.GetTimeSinceLastMix();
		time -= AudioServer.GetOutputLatency();

		step = (int)(time * bpm / 60.0);
		if (step > lastStep) {
			EmitSignalStep(step % 4);
			lastStep = step;
		}
		if (step % 4 == 0) {
			EmitSignalBeat(step / 4);
			lastStep = step;
		}
	}
}

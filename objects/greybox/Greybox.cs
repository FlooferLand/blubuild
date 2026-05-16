using System.Collections.Generic;
using Godot;

namespace Project;

public partial class Greybox : Node3D {
	[Signal] public delegate void ShowLoadedEventHandler();
	[Signal] public delegate void SignalFrameEventHandler(Bit[] frame, StringName chart);

	[Export] public required ReelToReel ReelToReel;

	public double Seek => ReelToReel.Seek;
	double lastFrameSeek = 0;
	int seekInt = 0;

	public override void _Ready() {
		ReelToReel.ShowLoaded += () => {
			lastFrameSeek = 0;
			seekInt = 0;
			EmitSignalShowLoaded();
		};
	}

	// Server-only
	public override void _PhysicsProcess(double delta) {
		if (ReelToReel.ServerLoadedShow is not { } loadedShow) return;
		if (ReelToReel.ServerLoadedChart is not { } mapping) return;

		while (Seek >= lastFrameSeek + ReelToReel.FrameDuration) {
			lastFrameSeek += ReelToReel.FrameDuration; // fixed step, not = Seek

			if (seekInt < loadedShow.signal.Length) {
				var frame = new List<Bit>();
				Bit i = seekInt;
				for (; i < loadedShow.signal.Length; i++) {
					Bit s = loadedShow.signal[i];
					if (s == 0) break;
					frame.Add(s);
				}
				Rpc(nameof(SendSignalFrame), frame.ToArray(), mapping);
				seekInt = i + 1;
			}
		}
	}

	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = TransferChannels.ShowSignal)]
	void SendSignalFrame(Bit[] frame, StringName mapping) {
		EmitSignalSignalFrame(frame, mapping);
	}
}

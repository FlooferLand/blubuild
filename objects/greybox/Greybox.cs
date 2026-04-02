using System.Collections.Generic;
using Godot;

namespace Project;

public partial class Greybox : Node3D {
	[Signal] public delegate void SignalFrameEventHandler(int[] frame);

	[Export] public required ReelToReel ReelToReel;

	public double Seek => ReelToReel.Seek;
	double lastFrameSeek = 0;
	int seekInt = 0;

	// Server-only
	public override void _PhysicsProcess(double delta) {
		if (ReelToReel.ServerLoadedShow is not { } loadedShow) return;

		while (Seek >= lastFrameSeek + ReelToReel.FrameDuration) {
			lastFrameSeek += ReelToReel.FrameDuration; // fixed step, not = Seek

			if (seekInt < loadedShow.signal.Length) {
				var frame = new List<int>();
				int i = seekInt;
				for (; i < loadedShow.signal.Length; i++) {
					int s = loadedShow.signal[i];
					if (s == 0) break;
					frame.Add(s);
				}
				Rpc(nameof(SendSignalFrame), frame.ToArray());
				seekInt = i + 1;
			}
		}
	}

	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = TransferChannels.ShowSignal)]
	void SendSignalFrame(int[] frame) {
		EmitSignalSignalFrame(frame);
	}
}

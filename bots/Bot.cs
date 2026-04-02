using System.Linq;
using Godot;

namespace Project;

public partial class Bot : Node3D {
	[Export] public required Greybox Greybox;

	[ExportGroup("Local")]
	[Export] public required BotData BotData;
	[Export] public required BotAnimationComp AnimationPlayer;

	public override void _Ready() {
		if (Multiplayer.IsClient()) {
			Greybox.SignalFrame += frame => {
				foreach ((int bit, BotMovement movement) in BotData.BitMapping) {
					AnimationPlayer.SetBit(bit, frame.Contains(bit));
				}
			};
		}
	}
}

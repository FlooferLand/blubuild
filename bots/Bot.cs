using System.Linq;
using Godot;

namespace Project;

public partial class Bot : Node3D {
	[Export] public required Greybox Greybox;

	[ExportGroup("Local")]
	[Export] public required BotAnimationComp AnimationPlayer;

	CharacterFile? Character = null;

	public void LoadCharacter(CharacterFile? file) {
		Character = file;
	}

	 public override void _Ready() {
		if (Multiplayer.IsClientOrIntegrated()) {
			Greybox.SignalFrame += (frame, mapping) => {
				if (Character?.BitData is not { } bitData) return;
				foreach (var (bit, data) in bitData.BitsToData) {
					AnimationPlayer.SetBit(bit, frame.Contains(bit.Bit));
				}
			};
		}
	}
}

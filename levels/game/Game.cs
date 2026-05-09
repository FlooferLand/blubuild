using Godot;

namespace Project;

public partial class Game : Node3D {
	public override void _Ready() {
		MusicGlobal.Stop();
	}
}

using Godot;
using GodotUtils;

namespace Project;

public partial class Init : Node {
	public override void _Ready() {
		if (!SystemAutoload.Instance.IsCommandlineOnly) {
			var returned = GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://levels/game.tscn");
			if (returned.Obj is Error err && err != Error.Ok) {
				Log.Error($"Error while opening scene: {err}");
			}
		}
	}
}

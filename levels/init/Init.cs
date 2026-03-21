using Godot;
using GodotUtils;

namespace Project;

public partial class Init : Node {
	[Export] public required PackedScene GameScene;

	public override void _Ready() {
		if (!SystemAutoload.Instance.IsCommandlineOnly) {
			var returned = GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToPacked, GameScene);
			if (returned.Obj is Error err && err != Error.Ok) {
				Log.Error($"Error while opening scene: {err}");
			}
		}
	}
}

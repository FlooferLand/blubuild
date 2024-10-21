using System;
using GodotUtils;
using Godot;

namespace Project.Components;

public partial class BotControllerComp : Node {
	[Export] public BotInfoComp[] Animatronics = Array.Empty<BotInfoComp>();
	[Export] public MechanicalAnimationPlayer AnimPlayer;
	
	public override void _Ready() {
		foreach (var animatronic in Animatronics) {
			var mesh = animatronic.Mesh;
			if (mesh?.GetNodeOrNull<AnimationPlayer>(nameof(AnimationPlayer)) is { } animationPlayer) {
				animationPlayer.Stop();  // Reset pose
			} else {
				Log.Error($"Couldn't get animation player for {animatronic.GetId()}");
			}
		}
	}

	public void TriggerAction(string animation, bool on) {
		AnimPlayer.PlayAnimation(animation, !on);
	}

	/*public override void _Input(InputEvent @event) {
		if (@event is InputEventKey key) {
			if (key.Echo) return;
			if (key.Keycode == Key.J) {
				if (key.IsPressed())
					TriggerAction("20 TD - Jaw", true);
				else if (key.IsReleased())
					TriggerAction("20 TD - Jaw", false);
			}
			if (key.Keycode == Key.U) {
				if (key.IsPressed())
					TriggerAction("10 TD - Head up", true);
				else if (key.IsReleased())
					TriggerAction("10 TD - Head up", false);
			}
			if (key.Keycode == Key.N) {
				if (key.IsPressed())
					TriggerAction("30 TD - Strum", true);
				else if (key.IsReleased())
					TriggerAction("30 TD - Strum", false);
			}
		}
	}*/
}

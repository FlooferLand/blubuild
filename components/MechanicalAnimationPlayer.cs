using System.Collections.Generic;

namespace Project.Components;
using Godot;

/**
 * Replacement for AnimationPlayer that uses math to animate basic physics.
 * It also allows for stacking of animations
 */
public partial class MechanicalAnimationPlayer : Node {
	[Export] public AnimationPlayer AnimPlayer;
	[Export] public Skeleton3D Skeleton;
	private Dictionary<StringName, RunningAnimation> activeAnimations = new();

	/** Describes the currently playing animation */
	public class RunningAnimation {
		public readonly Animation Animation;
		public double CurrentTime;
		public readonly bool Reversed;
		
		public RunningAnimation(Animation animation, bool reversed) {
			Animation = animation;
			CurrentTime = 0f;
			Reversed = reversed;
		}
	}
	
	public void PlayAnimation(StringName animation, bool reversed = false) {
		if (AnimPlayer.GetAnimation(animation) is not { } anim) return;
		activeAnimations.Remove(animation);
		activeAnimations.TryAdd(animation, new RunningAnimation(anim, reversed));
	}

	public void StopAnimation(StringName animation) {
		//bool reversed = activeAnimations[animation].Reversed;
		activeAnimations.Remove(animation);
		//if (!reversed) {
		//	PlayAnimation(animation, true);
		//}
	}

	public override void _Process(double delta) {
		foreach ((StringName animKey, var anim) in activeAnimations) {
			RunUpdate(delta, animKey, anim);
		}
	}

	public void RunUpdate(double delta, StringName animKey, RunningAnimation anim) {
		var animation = anim.Animation;
		double currTime = anim.CurrentTime;
		double animLength = animation.Length;
		if (currTime > animLength) {  // Animation finished
			StopAnimation(animKey);
			return;
		}
		
		for (int track = 0; track < animation.GetTrackCount(); track++) {
			// Timey-wimey stuff
			double time = currTime;
			if (anim.Reversed)
				time = animation.Length - currTime;
			
			// Fetching the nearest key
			int key = animation.TrackFindKey(track, time, Animation.FindMode.Nearest);
			if (key == -1 || key > animation.TrackGetKeyCount(track)) continue;

			// Handling rotation
			if (animation.TrackGetType(track) == Animation.TrackType.Rotation3D) {
				var value = animation.TrackGetKeyValue(track, key).AsQuaternion();
				var nextValue = key + 1 < animation.TrackGetKeyCount(track)
					? animation.TrackGetKeyValue(track, key + 1).AsQuaternion()
					: value;
				
				string boneName = animation.TrackGetPath(track).GetSubName(0);
				int boneId = Skeleton.FindBone(boneName);
				var restTransform = Skeleton.GetBoneRest(boneId);
				
				// Filter out empty tracks (only one keyframe)
				if (animation.TrackGetKeyCount(track) <= 1 && value.IsEqualApprox(restTransform.Basis.GetRotationQuaternion()))
					continue;

				// Making the new transform
				var transform = restTransform;
				value = value.Slerp(nextValue, 0.5f * (float) delta);
				transform.Basis = new Basis(value);
				transform.Origin = restTransform.Origin;
				
				if (transform != restTransform) Skeleton.SetBonePose(boneId, transform);
			}
		}
		anim.CurrentTime += delta;
	}
}

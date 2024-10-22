using System.Collections.Generic;
using System.Linq;

namespace Project.Components;
using Godot;

/**
 * Replacement for AnimationPlayer that uses math to animate basic physics.
 * It also allows for stacking of animations
 */
public partial class MechanicalAnimationPlayer : Node {
	[ExportGroup("Public")]
	[Export] public AnimationPlayer AnimPlayer;
	[Export] public Skeleton3D Skeleton;

	[ExportGroup("Private")]
	[Export] public AudioStream PneumaticFire;
	[Export] public AudioStream PneumaticRelease;
	
	private Dictionary<StringName, RunningAnimation> activeAnimations = new();
	private Option<Animation> defaultPose = Option<Animation>.None();
	private Dictionary<string, PneumaticAudioPair> pneumaticAudio = new();
	
	public record PneumaticAudioPair(AudioStreamPlayer3D Fire, AudioStreamPlayer3D Release);

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

	public override void _Ready() {
		// Finding the default pose animation name
		string[] defaultPoseNames = { "RESET", "Default", "Armature" };
		foreach (string pose in defaultPoseNames) {
			if (AnimPlayer.HasAnimation(pose)) {
				defaultPose = Option<Animation>.Some(AnimPlayer.GetAnimation(pose));
				break;
			}
		}
		
		// Creating an audio player for each moving bone (EXPENSIVE)
		// TODO: Add a settings toggle between this and using one single AudioStreamPlayer3d for performance reasons
		foreach (string animName in AnimPlayer.GetAnimationList()) {
			if (defaultPoseNames.Contains(animName)) continue;
			var anim = AnimPlayer.GetAnimation(animName);

			for (int trackId = 0; trackId < anim.GetTrackCount(); trackId++) {
				if (anim.TrackGetType(trackId) != Animation.TrackType.Rotation3D) continue;
				
				string boneName = anim.TrackGetPath(trackId).GetSubName(0)
					.Replace(" ", "_")
					.Replace(".", "_");
				if (Skeleton.GetNodeOrNull<BoneAttachment3D>(boneName) is { } boneAttachment) {
					AddPneumaticAudioSource(animName, boneAttachment);
				} else {
					var attachment = new BoneAttachment3D();
					attachment.BoneName = boneName;
					AddPneumaticAudioSource(animName, attachment);
					Skeleton.AddChild(attachment);
				}
			}
		}
	}

	public override void _Process(double delta) {
		foreach ((StringName animKey, var anim) in activeAnimations) {
			RunUpdate(delta, animKey, anim);
		}
	}

	// TODO: Add these audio players to the pneumatics audio mixing group
	private void AddPneumaticAudioSource(string animName, BoneAttachment3D parent) {
		var audioFire = new AudioStreamPlayer3D();
		audioFire.Stream = PneumaticFire;
		audioFire.MaxPolyphony = 3;
		audioFire.VolumeDb = -26;
		parent.AddChild(audioFire);
		
		var audioRelease = audioFire.Duplicate() as AudioStreamPlayer3D;
		audioRelease!.Stream = PneumaticRelease;
		parent.AddChild(audioRelease);
		
		pneumaticAudio.TryAdd(animName, new PneumaticAudioPair(audioFire, audioRelease));
	}
	
	public void PlayAnimation(StringName name, bool reversed = false) {
		if (!AnimPlayer.HasAnimation(name)) {
			GD.PushError($"Animation {name} not found on {AnimPlayer.GetParent().Name}");
			return;
		}
		activeAnimations.Remove(name);
		activeAnimations.TryAdd(name, new RunningAnimation(AnimPlayer.GetAnimation(name), reversed));
		if (pneumaticAudio.TryGetValue(name, out var audio)) {
			audio.Fire.Play();
		}
	}

	public void StopAnimation(StringName animation) {
		//bool reversed = activeAnimations[animation].Reversed;
		activeAnimations.Remove(animation);
		//if (!reversed) {
		//	PlayAnimation(animation, true);
		//}
		
		if (pneumaticAudio.TryGetValue(animation, out var audio)) {
			audio.Release.Play();
		}
	}

	public void StopAll() {
		foreach (var (animName, _) in activeAnimations) {
			StopAnimation(animName);
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
			
			// Fetching the default pose
			//if (defaultPose.LetSome(out var defaultPoseAnim)) {
			//	defaultPoseAnim.TrackGetKeyValue(defaultPoseAnim.find, 0);
			//}

			// Handling rotation
			if (animation.TrackGetType(track) == Animation.TrackType.Rotation3D) {
				var value = animation.TrackGetKeyValue(track, key).AsQuaternion();
				var nextValue = key + 1 < animation.TrackGetKeyCount(track)
					? animation.TrackGetKeyValue(track, key + 1).AsQuaternion()
					: value;
				
				string boneName = animation.TrackGetPath(track).GetSubName(0);
				int boneId = Skeleton.FindBone(boneName);
				var restTransform = Skeleton.GetBoneRest(boneId);
				
				// Filter out empty 1-frame long tracks (this removes the weird resetting to the rest pose)
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
